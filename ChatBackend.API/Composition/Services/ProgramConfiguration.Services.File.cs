using ChatBackend.Services;

namespace ChatBackend.Composition.Services
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder AddFileServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IFileService, FileService>();

            return builder;
        }
    }
}
