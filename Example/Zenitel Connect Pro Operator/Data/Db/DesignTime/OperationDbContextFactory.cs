using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace ZenitelConnectProOperator.Data.Db;

public sealed class OperationDbContextFactory : IDesignTimeDbContextFactory<OperationDbContext>
{
    public OperationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OperationDbContext>();

        // Keep this design-time path simple & deterministic
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(basePath, "Zenitel Connect Pro");
        Directory.CreateDirectory(folder);

        var dbPath = Path.Combine(folder, "Operation.db");

        options.UseSqlite($"Data Source={dbPath}");

        return new OperationDbContext(options.Options);
    }
}
