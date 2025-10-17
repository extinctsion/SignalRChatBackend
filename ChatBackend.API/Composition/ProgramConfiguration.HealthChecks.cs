using ChatBackend.Data;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder)
        {
            
            builder.Services.AddHealthChecks()
            .AddDbContextCheck<ChatDbContext>()
            .AddCheck("redis", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
            
            return builder;
        }
    }
}
