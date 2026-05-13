using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.Data.Db;

namespace ZenitelConnectProOperator.Core.Services;

/// <summary>
/// EF-backed configuration store.
/// Wraps OperationDbContext and exposes only the configuration
/// operations required by the core runtime.
/// </summary>
public sealed class EfConfigStore : IConfigStore
{
    private readonly IDbContextFactory<OperationDbContext> _dbFactory;

    public EfConfigStore(IDbContextFactory<OperationDbContext> dbFactory)
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
            // Config initialization must never crash the app
        }
    }

    public ConnectPro.Configuration? LoadForMachine(string machineName)
    {
        try
        {
            using var db = _dbFactory.CreateDbContext();

            // Exact equivalent of the example PluginContext.Configuration.FirstOrDefault(...)
            return db.Configurations
                     .AsNoTracking()
                     .FirstOrDefault(c => c.MachineName == machineName);
        }
        catch
        {
            return null;
        }
    }

    public void SaveConfiguration(ConnectPro.Configuration configuration)
    {
        try
        {
            using var db = _dbFactory.CreateDbContext();

            // Check if configuration already exists for this machine
            var existing = db.Configurations
                             .FirstOrDefault(c => c.MachineName == configuration.MachineName);

            if (existing != null)
            {
                // Update existing
                existing.ServerAddr = configuration.ServerAddr;
                existing.Port = configuration.Port;
                existing.Realm = configuration.Realm;
                existing.UserName = configuration.UserName;
                existing.Password = configuration.Password;
                existing.OperatorDirNo = configuration.OperatorDirNo;
            }
            else
            {
                // Insert new
                db.Configurations.Add(configuration);
            }

            db.SaveChanges();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            throw;
        }
    }
}
