using Serilog;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .WriteTo.File("logs/chatbackend-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            
            builder.Host.UseSerilog();
        }
    }
}
