using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConnectPro.Models;
using Microsoft.EntityFrameworkCore;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.Data.Db;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class EfLogStore : ILogStore
{
    private readonly IDbContextFactory<LogDbContext> _dbFactory;

    public EfLogStore(IDbContextFactory<LogDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public void Initialize()
    {
        try
        {
            using var db = _dbFactory.CreateDbContext();
            db.Database.Migrate();
        }
        catch
        {
            // Logging must never break the app
        }
    }

    public void LogException(string sender, Exception ex)
    {
        try
        {
            using var db = _dbFactory.CreateDbContext();

            var log = new ExceptionLog();

            // Set fields only if they exist on the model.
            // This avoids brittle code while you migrate models.
            TrySet(log, "Source", sender);
            TrySet(log, "Sender", sender);
            TrySet(log, "Origin", sender);

            TrySet(log, "Message", ex.Message);
            TrySet(log, "ExceptionMessage", ex.Message);

            TrySet(log, "StackTrace", ex.ToString());
            TrySet(log, "Details", ex.ToString());
            TrySet(log, "Exception", ex.ToString());

            // Prefer UTC if model supports it
            TrySet(log, "CreatedUtc", DateTime.UtcNow);
            TrySet(log, "TimeUtc", DateTime.UtcNow);

            // Fall back to local time if that’s what the entity uses
            TrySet(log, "Created", DateTime.Now);
            TrySet(log, "Time", DateTime.Now);

            db.ExceptionLogs.Add(log);
            db.SaveChanges();
        }
        catch
        {
            // Never throw from logging
        }
    }

    public async Task<IReadOnlyList<CallLog>> GetLatestCallLogsAsync(int take)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            return await db.CallLogs
                .AsNoTracking()
                .OrderByDescending(x => x.Time)
                .Take(take)
                .ToListAsync();
        }
        catch
        {
            // Don’t break UI if DB read fails
            return Array.Empty<CallLog>();
        }
    }

    public async Task<bool> ExistsSimilarAsync(CallLog entry, DateTime windowStartUtcOrLocal)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            return await db.CallLogs
                .AsNoTracking()
                .Where(x => x.Time >= windowStartUtcOrLocal)
                .AnyAsync(x =>
                    x.DeviceName == entry.DeviceName &&
                    x.FromDirno == entry.FromDirno &&
                    x.ToDirno == entry.ToDirno &&
                    x.AnsweredByDirno == entry.AnsweredByDirno &&
                    x.State == entry.State);
        }
        catch
        {
            // If we can’t check, assume it exists to avoid spam writes
            return true;
        }
    }

    public async Task AddAsync(CallLog entry)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            await db.CallLogs.AddAsync(entry);
            await db.SaveChangesAsync();
        }
        catch
        {
            // Never throw from logging store
        }
    }

    private static void TrySet(object target, string propertyName, object? value)
    {
        if (value is null) return;

        var p = target.GetType().GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (p is null || !p.CanWrite) return;

        var targetType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

        try
        {
            if (targetType.IsInstanceOfType(value))
            {
                p.SetValue(target, value);
                return;
            }

            // Attempt conversion for common primitives
            var converted = Convert.ChangeType(value, targetType);
            p.SetValue(target, converted);
        }
        catch
        {
            // Ignore any setter/conversion issues
        }
    }
}
