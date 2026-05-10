using Microsoft.EntityFrameworkCore;
using WeatherAPI.Models;

namespace WeatherAPI.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherData> WeatherData { get; set; }
    public DbSet<WeatherAlert> WeatherAlerts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserConsent> UserConsents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for performance
        modelBuilder.Entity<WeatherData>()
            .HasIndex(w => w.Location);

        modelBuilder.Entity<WeatherData>()
            .HasIndex(w => w.Timestamp);

        modelBuilder.Entity<WeatherAlert>()
            .HasIndex(w => w.Location);

        modelBuilder.Entity<WeatherAlert>()
            .HasIndex(w => w.IsActive);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.UserId);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);

        modelBuilder.Entity<UserConsent>()
            .HasIndex(u => u.UserId);

        // Configure data retention
        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<WeatherData>()
            .Property(w => w.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}