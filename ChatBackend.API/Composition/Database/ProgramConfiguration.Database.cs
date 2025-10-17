using ChatBackend.Composition.Database;
using Microsoft.EntityFrameworkCore;


namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder)
        {
            builder.ConfigurePostgresDatabase();

            return builder;
        }
    }
}
