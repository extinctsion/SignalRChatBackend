using Microsoft.OpenApi;

namespace ChatBackend.Composition.OpenAPI
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureDocumentTransformers(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            Description = "JWT Authorization header using the Bearer scheme. Enter your token directly.",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT"
                        }
                    };




                    return Task.CompletedTask;
                });
            });

            return builder;
        }
    }
}
