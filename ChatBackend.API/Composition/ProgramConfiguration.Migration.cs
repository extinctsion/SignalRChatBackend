using ChatBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatBackend.Composition
{
    internal static partial class ProgramConfiguration
    {
        public static WebApplication ApplyMigration(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                context.Database.Migrate();
            }

            return app;
        }
    }
}
