using GameServer;
using GameServer.Hubs;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
           .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); 
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHub<GameHub>("/Hub");

app.UseCors("AllowAll");

app.Run();
