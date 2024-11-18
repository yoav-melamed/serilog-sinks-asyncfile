using Serilog.Events;
using Xunit.Sdk;

namespace Serilog.Sinks.AsyncFile.Tests.Utils;

public static class LogUtils
{
    public static LogEvent CreateLogEvent(string messageTemplate, params object[] parameters)
    {
        var log = new LoggerConfiguration().CreateLogger();
        if (!log.BindMessageTemplate(messageTemplate, parameters, out var template, out var properties))
        {
            throw new XunitException("Template could not be bound.");
        }

        return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, template, properties);
    }
}