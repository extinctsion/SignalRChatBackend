using ChatBackend.Composition.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
        {

            builder.ConfigureJWTAuthentication();
            
            builder.ConfigureSignalRAuthentication();

            return builder;
        }

     }
}
