using Serilog.Formatting.Display;
using Serilog.Formatting.Json;
using Serilog.Sinks.AsyncFile.Tests.Utils;

namespace Serilog.Sinks.AsyncFile.Tests;

public class AsyncFileSinkTests
{
    [Fact]
    public async Task Emit_WriteLineWithDefaultOptions_LineIsWritten()
    {
        // Arrange
        const string logMessage = "Test log message";

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();

        using var asyncFileSink = new AsyncFileSink(path: tempFilePath);
        var logEvent = LogUtils.CreateLogEvent(logMessage);

        // Act
        asyncFileSink.Emit(logEvent);
        await Task.Delay(150);

        var firstLine = await tempUtils.ReadFirstLine(tempFilePath);

        // Assert
        Assert.Contains(logMessage, firstLine);
    }

    [Fact]
    public async Task Emit_WriteMoreLogsThanQueueCapacity_AllLinesAreWritten()
    {
        // Arrange
        const string logMessage = "Test log message";
        const int logCount = 100;

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();
        var logEvent = LogUtils.CreateLogEvent(logMessage);

        // Act
        await using var asyncFileSink = new AsyncFileSink(tempFilePath, 1);
        {
            Parallel.For(1, logCount, _ => { asyncFileSink.Emit(logEvent); });
        }
        
        asyncFileSink.Emit(logEvent);
        await Task.Delay(150);

        var firstLine = await tempUtils.ReadFirstLine(tempFilePath);

        // Assert
        var logLines = await File.ReadAllLinesAsync(tempFilePath);
        Assert.Equal(logCount, logLines.Length);
    }

    [Fact]
    public async Task Emit_WriteLineWithCustomFormatter_LineIsWritten()
    {
        // Arrange
        const string logMessage = "Test log message";
        var customFormatter = new MessageTemplateTextFormatter("{Message}");

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();

        using var asyncFileSink = new AsyncFileSink(path: tempFilePath, customFormatter);

        var logEvent = LogUtils.CreateLogEvent(logMessage);

        // Act
        asyncFileSink.Emit(logEvent);
        await Task.Delay(150);

        var firstLine = await tempUtils.ReadFirstLine(tempFilePath);

        // Assert
        Assert.Equal(logMessage, firstLine);
    }

    [Fact]
    public async Task Emit_WriteLineWithAllOptions_LineIsWritten()
    {
        // Arrange
        const string logMessage = "Test log message";
        var customFormatter = new MessageTemplateTextFormatter("[{Level:u3}] {Message}");

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();

        using var asyncFileSink = new AsyncFileSink(tempFilePath, customFormatter, 1);

        var logEvent = LogUtils.CreateLogEvent(logMessage);

        // Act
        asyncFileSink.Emit(logEvent);
        await Task.Delay(150);

        var firstLine = await tempUtils.ReadFirstLine(tempFilePath);

        // Assert
        Assert.Equal($"[INF] {logMessage}", firstLine);
    }

    [Fact]
    public async Task Emit_WriteLineWithRollOnStartup_LineIsWritten()
    {
        // Arrange
        const string logMessage = "Test log message";

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();
        await File.WriteAllTextAsync(tempFilePath, logMessage);

        var historyFolder = Path.Combine(Path.GetDirectoryName(tempFilePath)!, "History");

        // Act
        using var asyncFileSink = new AsyncFileSink(tempFilePath, new RollingPolicyOptions
        {
            RollOnStartup = true,
            RollToArchiveFolder = true,
            ArchiveFolderName = "History"
        });

        // Assert
        Assert.True(Directory.Exists(historyFolder));

        var historyFilePath = Directory.GetFiles(historyFolder)[0];
        Assert.True(File.Exists(historyFilePath));

        // Cleanup
        File.Delete(historyFilePath);
        Directory.Delete(historyFolder, true);

        Assert.False(File.Exists(historyFilePath));
        Assert.False(Directory.Exists(historyFolder));
    }

    [Fact]
    public async Task Emit_CheckAllAsyncFileSinkConstructorOptions_LineIsWritten()
    {
        // Arrange
        const string logMessage = "Test log message";

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();
        await File.WriteAllTextAsync(tempFilePath, logMessage);

        var historyFolder = Path.Combine(Path.GetDirectoryName(tempFilePath)!, "History");

        // Act
        await using var asyncFileSink = new AsyncFileSink(tempFilePath, 1, new JsonFormatter(), new RollingPolicyOptions
        {
            RollOnStartup = true,
            RollToArchiveFolder = true,
            ArchiveFolderName = "History",
            RollingFileNameFormat = "{0:yyyy-MM-dd-HH-mm-ss-fff}-{1}{2}",
            RollingRetentionDays = 1
        });

        // Assert
        Assert.True(Directory.Exists(historyFolder));

        var historyFilePath = Directory.GetFiles(historyFolder)[0];
        Assert.True(File.Exists(historyFilePath));

        // Cleanup
        File.Delete(historyFilePath);
        Directory.Delete(historyFolder, true);

        Assert.False(File.Exists(historyFilePath));
        Assert.False(Directory.Exists(historyFolder));
    }

    [Fact]
    public void Emit_WriteLogWhenLogsFolderDoesNotExist_LogsFolderIsCreated()
    {
        // Arrange
        const string logMessage = "Test log message";
        var logEvent = LogUtils.CreateLogEvent(logMessage);

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();
        var tempFolder = Path.GetDirectoryName(tempFilePath)!;
        var logsFolder = Path.Combine(tempFolder, "logs");
        var logFilePath = Path.Combine(logsFolder, Path.GetFileName(tempFilePath));

        // Act
        using var asyncFileSink = new AsyncFileSink(logFilePath);
        asyncFileSink.Emit(logEvent);

        // Assert
        Assert.True(Directory.Exists(logsFolder));

        // Cleanup
        Directory.Delete(logsFolder, true);
        Assert.False(Directory.Exists(logsFolder));
    }

    [Fact]
    public async Task Emit_StressTest_WriteLogsInParallel()
    {
        // Arrange
        const int logCount = 50_000;
        const string logMessage = "Test log message";

        using var tempUtils = new FileSystemUtils();
        var tempFilePath = tempUtils.GenerateTempFilePath();
        var logEvent = LogUtils.CreateLogEvent(logMessage);

        // Act
        await using (var asyncFileSink = new AsyncFileSink(tempFilePath))
        {
            Parallel.For(0, logCount, _ => { asyncFileSink.Emit(logEvent); });
        }

        // Assert
        var logLines = await File.ReadAllLinesAsync(tempFilePath);
        Assert.Equal(logCount, logLines.Length);
    }
}