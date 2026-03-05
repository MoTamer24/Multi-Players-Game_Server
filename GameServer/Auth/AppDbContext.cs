using Microsoft.EntityFrameworkCore;

using GameServer.Auth.Models;

namespace GameServer.Auth
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<MatchRecord> MatchRecords { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens{get;set;}=null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(u => new { u.Provider, u.ProviderId })  // makes sure no two rows has same provider and same id 
                .IsUnique();

            modelBuilder.Entity<UserConnection>()
                .HasKey(c => c.ConnectionId);

            modelBuilder.Entity<UserConnection>()
                .HasIndex(c => c.UserId);
            
            modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.Token)
            .IsUnique();
        }
    }
}
