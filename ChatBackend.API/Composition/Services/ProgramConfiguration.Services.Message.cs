using ChatBackend.Services;

namespace ChatBackend.Composition.Services
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddMessageServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IMessageService, MessageService>();

            return builder;
        }
    }
}
