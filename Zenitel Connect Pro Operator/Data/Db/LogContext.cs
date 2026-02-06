using ConnectPro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace ZenitelConnectProOperator.Data.Db;

/// <summary>
/// Logging database context.
/// Stores call logs and exception logs in a dedicated SQLite database,
/// separated from plugin/configuration data.
/// </summary>
public sealed class LogDbContext : DbContext
{
    public DbSet<CallLog> CallLogs => Set<CallLog>();
    public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();

    public LogDbContext(DbContextOptions<LogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<CallLog>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
        });
    }

    /// <summary>
    /// Applies migrations and ensures the log database is usable.
    /// Safe to call multiple times.
    /// </summary>
    public void InitializeDatabase()
    {
        try
        {
            var applied = Database.GetAppliedMigrations().Any();
            var pending = Database.GetPendingMigrations().Any();

            if (!applied || pending)
            {
                Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            // IMPORTANT:
            // Logging must never throw back into the app.
            // Last-resort fallback: write to local file.
            try
            {
                File.AppendAllText(
                    Path.Combine(AppContext.BaseDirectory, "logdb-fatal.txt"),
                    $"{DateTime.UtcNow:u} {ex}\n");
            }
            catch
            {
                // swallow absolutely everything
            }
        }
    }

    /// <summary>
    /// Standardized path resolver for Log.db
    /// </summary>
    public static string GetDefaultDbPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(basePath, "Zenitel Connect Pro");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return Path.Combine(folder, "Log.db");
    }
}
