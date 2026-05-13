using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZenitelConnectProOperator.Data.Db;

public sealed class LogDbContextFactory : IDesignTimeDbContextFactory<LogDbContext>
{
    public LogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LogDbContext>();

        // You already have a standardized resolver here:
        var dbPath = LogDbContext.GetDefaultDbPath();

        options.UseSqlite($"Data Source={dbPath}");

        return new LogDbContext(options.Options);
    }
}
