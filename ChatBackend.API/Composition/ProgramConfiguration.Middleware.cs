using ChatBackend.Hubs;

namespace ChatBackend.Composition
{
    internal static partial class  ProgramConfiguration
    {
        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHealthChecks("/health");

            return app;
        }
    }
}
