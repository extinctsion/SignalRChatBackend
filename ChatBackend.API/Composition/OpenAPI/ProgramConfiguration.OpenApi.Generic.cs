using Scalar.AspNetCore;

namespace ChatBackend.Composition.OpenAPI
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplication ConfigureOpenApiDoc(this WebApplication app)
        {
            app.MapOpenApi();
            
            app.MapScalarApiReference(opt =>
            {
                opt.Title = "Chatbackend API";
                opt.Theme = ScalarTheme.Mars;
                opt.PersistentAuthentication = true;
            });

            return app;
        }
    }
}
