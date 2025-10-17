using ChatBackend.Services;
using ChatBackend.Composition.Services;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.AddUserServices();
            builder.AddMessageServices();
            builder.AddConversationServices();
            builder.AddFileServices();

            return builder;
        }
    }
}
