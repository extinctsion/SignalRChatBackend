using Serilog;
using Serilog.Context;
using ILogger = Serilog.ILogger;

namespace ChatBackend.Services
{
    public class AppLoggerService : IAppLogger
    {
        private readonly ILogger _logger;
        public AppLoggerService()
        {
            _logger = Log.Logger.ForContext("App", "ChatBackendAPI");
        }

        public void Info(string message, object? context = null) => PushContext(context, () => _logger.Information(message));
        public void Warn(string message, object? context = null) => PushContext(context, () => _logger.Warning(message));
        public void Error(string message, Exception? ex = null, object? context = null) => PushContext(context, () => _logger.Error(ex, message));
        public void Debug(string message, object? context = null) => PushContext(context, () => _logger.Debug(message));

        public void LogBusinessEvent(string message, object? context = null) => PushContext(context, () => _logger
             .ForContext("EventType", "BusinessEvent")
             .Information(message)); public void TrackExecutionTime(string operation, TimeSpan duration, object? context = null) => PushContext(context, () => _logger.Information("{Operation} took {Duration}ms", operation, duration.TotalMilliseconds));

        private void PushContext(object? context, Action logAction)
        {
            using (LogContext.PushProperty("Context", context, destructureObjects: true))
            {
                logAction();
            }
        }

    }
}
