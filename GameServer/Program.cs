using GameServer;
using GameServer.Application.Interfaces;
using GameServer.Auth;
using GameServer.Domain.Factories;
using GameServer.Hubs;
using GameServer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load secrets.json if present in project directory, and load dotnet user-secrets
builder.Configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

var config = builder.Configuration;

// 1. Connection string & Persistence
string connectionString = config.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? "Data Source=gameserver.db";

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connectionString));

// 2. Domain & Application Services (Clean Architecture DI)
builder.Services.AddSingleton<IGameFactory, GameFactory>();
builder.Services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
builder.Services.AddSingleton<IMatchmakingService, InMemoryMatchmakingService>();
builder.Services.AddSingleton<IGameSessionService, InMemoryGameSessionService>();

// Legacy facade for backward compatibility
builder.Services.AddSingleton<GameManager>();

// Auth Services
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// 3. Transport & API
builder.Services.AddSignalR();
builder.Services.AddControllers();

// 4. CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:8080", "http://localhost:5173" };

    options.AddPolicy("AllowAll", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// 5. Authentication & JWT Validation
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = AuthConstants.GetAuthParameters(config);

    // Allow JWT via query string for SignalR negotiate
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// 6. Pipeline configuration
app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<GameHub>("/Hub").RequireAuthorization();

// 7. Auto-apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<AppDbContext>();
    db?.Database.Migrate();
}

app.Run();
