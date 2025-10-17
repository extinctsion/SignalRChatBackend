using ChatBackend.Composition.OpenAPI;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureOpenApi(this WebApplicationBuilder builder)
        {
            builder.ConfigureDocumentTransformers();

            return builder;
        }

        public static WebApplication UseOpenApi(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
               app.ConfigureOpenApiDoc();
            }

            return app;
        }

    }
}
