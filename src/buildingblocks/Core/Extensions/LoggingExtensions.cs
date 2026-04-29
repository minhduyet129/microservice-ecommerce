using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Core.Extensions;

public static class LoggingExtensions
{
    public static LoggerConfiguration AddCustomLogging(this LoggerConfiguration loggerConfiguration, string serviceName)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.FromLogContext();
    }
}