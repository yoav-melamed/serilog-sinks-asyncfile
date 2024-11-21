using Serilog.Configuration;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.AsyncFile;

/// <summary>
/// Extends <see cref="LoggerSinkConfiguration"/> with methods to write log events to a file.
/// </summary>
public static class LoggerConfigurationExtension
{
    private const int DefaultCapacity = 65_536;
    private const string DefaultOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path)
    {
        return AsyncFile(loggerSinkConfiguration, path, DefaultCapacity, DefaultOutputTemplate, new RollingPolicyOptions());
    }

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="capacity">The maximum queue capacity of the producer-consumer writer. Default: 65,536</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path,
        int capacity)
    {
        return AsyncFile(loggerSinkConfiguration, path, capacity, DefaultOutputTemplate, new RollingPolicyOptions());
    }

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.
    /// the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}".</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path,
        string outputTemplate)
    {
        return AsyncFile(loggerSinkConfiguration, path, DefaultCapacity, outputTemplate, new RollingPolicyOptions());
    }

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="rollingPolicyOptions">Rolling policy definition.</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path, 
        RollingPolicyOptions rollingPolicyOptions)
    {
        return AsyncFile(loggerSinkConfiguration, path, DefaultCapacity, DefaultOutputTemplate, rollingPolicyOptions);
    }

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.
    /// the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}".</param>
    /// <param name="rollingPolicyOptions">Rolling policy definition.</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path,
        string outputTemplate,
        RollingPolicyOptions rollingPolicyOptions)
    {
        return AsyncFile(loggerSinkConfiguration, path, DefaultCapacity, outputTemplate, rollingPolicyOptions);
    }

    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.
    /// the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}".</param>
    /// <param name="capacity">The maximum queue capacity of the producer-consumer writer. Default: 65,536</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path,
        string outputTemplate, 
        int capacity)
    {
        return AsyncFile(loggerSinkConfiguration, path, capacity, outputTemplate, new RollingPolicyOptions());
    }
        
    /// <summary>
    /// Write log events to a file.
    /// </summary>
    /// <param name="loggerSinkConfiguration"></param>
    /// <param name="path">Path to the file.</param>
    /// <param name="capacity">The maximum queue capacity of the producer-consumer writer. Default: 65,536</param>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.
    /// the default is "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}".</param>
    /// <param name="rollingPolicyOptions">Rolling policy definition.</param>
    /// <returns></returns>
    public static LoggerConfiguration AsyncFile(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string path, 
        int capacity, 
        string outputTemplate,
        RollingPolicyOptions rollingPolicyOptions)
    {
        var formatter = new MessageTemplateTextFormatter(outputTemplate);
        return loggerSinkConfiguration.Sink(new AsyncFileSink(path, capacity, formatter, rollingPolicyOptions));
    }
}