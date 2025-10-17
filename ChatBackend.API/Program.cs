using ChatBackend.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureOpenApi();
builder.ConfigureDatabase();
builder.ConfigureAuthentication();
builder.ConfigureCors();
builder.ConfigureSignalR();
builder.ConfigureAWS();
builder.AddServices();
builder.AddHealthChecks();
   
var app = builder.Build();

app.UseOpenApi();
app.ConfigureMiddleware();
app.ApplyMigration();

app.Run();
