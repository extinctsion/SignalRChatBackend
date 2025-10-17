using ChatBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Composition.Database
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplicationBuilder ConfigurePostgresDatabase(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseNpgsql(connectionString));

            return builder;
        }
    }
}
