using ChatBackend.Services;

namespace ChatBackend.Composition.Services
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddUserServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();

            return builder;
        }
    }
}
