using GameServer;
using GameServer.Hubs;
using GameServer.Auth.Models;
using GameServer.Auth.Services;
using GameServer.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var config = builder.Configuration;
string connectionString = config.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? "Data Source=gameserver.db";  // for dev only 

// Core services  
builder.Services.AddSignalR();
builder.Services.AddControllers();  
builder.Services.AddSingleton<GameManager>();

// Persistence
builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connectionString));

// Auth DI (implementations added in Auth/Services) 
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, ClaimUserIdProvider>();

// CORS (keep restrictive in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// Authentication - application JWTs (guests + exchanged IdP users)
var jwtKey = config["JWT:SigningKey"];
if (string.IsNullOrEmpty(jwtKey))
{
    // Development fallback (not for production)
    jwtKey = "dev-key-please-replace-in-prod-CHANGE_THIS_TO_ENV_VAR";
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = AuthConstants.GetAuthParameters(config);

    // Allow the JWT to be passed in the query string for SignalR negotiate (dev-friendly)
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

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<GameHub>("/Hub").RequireAuthorization();

// Apply pending migrations at startup for single-instance deployments (safe-guarded)
// avoid the headache of manually updating database after any change in Entities 
// it do it automatically 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<AppDbContext>();
    db?.Database.Migrate();
}

app.Run();
