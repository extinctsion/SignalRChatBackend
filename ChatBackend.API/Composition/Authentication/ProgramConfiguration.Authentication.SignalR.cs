using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChatBackend.Composition.Authentication
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureSignalRAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return builder;
        }
    }
}
