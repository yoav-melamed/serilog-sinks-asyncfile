using System.Text;
using System.Threading.Channels;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.AsyncFile;

/// <summary>
/// A sink that writes log events to a file.
/// </summary>
public sealed class AsyncFileSink : ILogEventSink, IDisposable
{
    private FileStream _underlingFileStream;
    private StreamWriter _writer;

    private readonly string _logPath;
    private readonly Channel<LogEvent> _logQueue;
    private readonly ITextFormatter _formatter;
    private readonly RollingPolicyOptions _rollingPolicyOptions;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _consumerTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    public AsyncFileSink(string path)
        : this(path, 65_536, new JsonFormatter(), new RollingPolicyOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="capacity"></param>
    public AsyncFileSink(string path, int capacity)
        : this(path, capacity, new JsonFormatter(), new RollingPolicyOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="formatter"></param>
    public AsyncFileSink(string path, ITextFormatter formatter)
        : this(path, 65_536, formatter, new RollingPolicyOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rollingPolicyOptions"></param>
    public AsyncFileSink(string path, RollingPolicyOptions rollingPolicyOptions)
        : this(path, 65_536, new JsonFormatter(), rollingPolicyOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="formatter"></param>
    /// <param name="rollingPolicyOptions"></param>
    public AsyncFileSink(string path, ITextFormatter formatter, RollingPolicyOptions rollingPolicyOptions)
        : this(path, 65_536, formatter, rollingPolicyOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="formatter"></param>
    /// <param name="capacity"></param>
    public AsyncFileSink(string path, ITextFormatter formatter, int capacity)
        : this(path, capacity, formatter, new RollingPolicyOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileSink"/> class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="capacity"></param>
    /// <param name="formatter"></param>
    /// <param name="rollingPolicyOptions"></param>
    public AsyncFileSink(string path, int capacity, ITextFormatter formatter, RollingPolicyOptions rollingPolicyOptions)
    {
        _rollingPolicyOptions = rollingPolicyOptions;
        _cancellationTokenSource = new CancellationTokenSource();

        _logPath = path;
        _formatter = formatter;

        if (rollingPolicyOptions.RollOnStartup)
        {
            RollFile(path, rollingPolicyOptions);
        }

        _logQueue = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        var logsFolder = Path.GetDirectoryName(path);
        if (!Directory.Exists(logsFolder))
            Directory.CreateDirectory(logsFolder!);

        _underlingFileStream =
            new FileStream(_logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

        _writer = new StreamWriter(_underlingFileStream, Encoding.UTF8);

        _ = RunRollingCleanerBackgroundTask(rollingPolicyOptions.AgeCheckInterval);
        _consumerTask = ConsumeMessages();
    }

    /// <summary>
    /// Emit a log event to the sink.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <exception cref="Exception"></exception>
    public void Emit(LogEvent logEvent)
    {
        if (!_logQueue.Writer.TryWrite(logEvent))
            throw new Exception("Failed to write log message to the AsyncFile Sink queue.");
    }

    /// <summary>
    /// Disposes the sink.
    /// </summary>
    public void Dispose()
    {
        _logQueue.Writer.TryComplete();
        _consumerTask.Wait();

        _writer.Dispose();
        _cancellationTokenSource.Dispose();
    }

    #region Private Methods

    private async Task ConsumeMessages()
    {
        await foreach (var logEvent in _logQueue.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            try
            {
                await WriteMessageAndFlush(logEvent);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Error writing log event to the AsyncFile Sink: {0}", ex);
            }
        }
    }

    private async Task WriteMessageAndFlush(LogEvent logEvent)
    {
        if (_rollingPolicyOptions.FileSizeLimitBytes > 0)
        {
            var fileSize = _underlingFileStream.Length;
            if (fileSize >= _rollingPolicyOptions.FileSizeLimitBytes)
            {
                _writer.Close();
                RollFile(_underlingFileStream.Name, _rollingPolicyOptions);

                _underlingFileStream =
                    new FileStream(_logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

                _writer = new StreamWriter(_underlingFileStream, Encoding.UTF8);
            }
        }

        var logFolderPath = Path.GetDirectoryName(_logPath);
        if (!Directory.Exists(logFolderPath))
            Directory.CreateDirectory(logFolderPath!);

        _formatter.Format(logEvent, _writer);
        await _writer.FlushAsync();
    }

    private async Task RunRollingCleanerBackgroundTask(int checkIntervalSec)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var rollingFolderName = _rollingPolicyOptions.RollToArchiveFolder
                ? _rollingPolicyOptions.ArchiveFolderName
                : "";

            var rollingFolderPath = Path.Combine(Path.GetDirectoryName(_logPath)!, rollingFolderName);

            if (!Directory.Exists(rollingFolderPath))
            {
                await Task.Delay(TimeSpan.FromSeconds(checkIntervalSec), _cancellationTokenSource.Token);
                continue;
            }

            var rollingFiles = Directory
                .GetFiles(rollingFolderPath, "*")
                .Where(f => f != _logPath);

            foreach (var rolledFile in rollingFiles)
            {
                var creationTime = File.GetCreationTime(rolledFile);
                if (creationTime < DateTime.Now.AddDays(-_rollingPolicyOptions.RollingRetentionDays))
                {
                    File.Delete(rolledFile);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(checkIntervalSec), _cancellationTokenSource.Token);
        }
    }

    private static void RollFile(
        string path,
        RollingPolicyOptions rollingPolicyOptions)
    {
        if (!File.Exists(path)) return;

        var rollingFolderName = rollingPolicyOptions.RollToArchiveFolder
            ? rollingPolicyOptions.ArchiveFolderName
            : "";

        var rollingFolderPath = Path.Combine(Path.GetDirectoryName(path)!, rollingFolderName);
        if (!Directory.Exists(rollingFolderPath))
            Directory.CreateDirectory(rollingFolderPath);

        var rollingFileNameFormat = rollingPolicyOptions.RollingFileNameFormat;
        var rollingFilePath = Path.Combine(
            rollingFolderPath,
            string.Format(
                rollingFileNameFormat,
                DateTime.Now,
                Path.GetFileNameWithoutExtension(path),
                Path.GetExtension(path)));

        File.Move(path, rollingFilePath);
    }

    #endregion
}