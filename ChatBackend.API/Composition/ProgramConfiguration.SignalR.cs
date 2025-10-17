namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureSignalR(this WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR()
            .AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");
            
            return builder;
        }
    }
}
