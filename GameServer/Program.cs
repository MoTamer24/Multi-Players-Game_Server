using GameServer;
using GameServer.Hubs;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHub<GameHub>("/Hub");

app.Run();
