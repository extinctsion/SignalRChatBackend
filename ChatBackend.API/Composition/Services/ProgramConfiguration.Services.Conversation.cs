using ChatBackend.Services;

namespace ChatBackend.Composition.Services
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddConversationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IConversationService, ConversationService>();

            return builder;
        }
    }
}
