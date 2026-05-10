using Microsoft.EntityFrameworkCore;
using WeatherApi.Models;

namespace WeatherApi.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherData> WeatherData { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserConsent> UserConsents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WeatherData
        modelBuilder.Entity<WeatherData>(entity =>
        {
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsArchived);
            
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
            entity.Property(e => e.FeelsLike).HasPrecision(5, 2);
            entity.Property(e => e.Pressure).HasPrecision(6, 2);
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.IsRetained);
        });

        // Configure UserConsent
        modelBuilder.Entity<UserConsent>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ConsentType);
            entity.HasIndex(e => new { e.UserId, e.ConsentType });
        });
    }
}