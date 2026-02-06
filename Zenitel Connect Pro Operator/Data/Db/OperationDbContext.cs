using ConnectPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ZenitelConnectProOperator.Data.Db;

public sealed class OperationDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<ConnectPro.Configuration> Configurations => Set<ConnectPro.Configuration>();
    public DbSet<Operator> Operators => Set<Operator>();

    public OperationDbContext(DbContextOptions<OperationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Operator entity
        modelBuilder.Entity<Operator>(entity =>
        {
            entity.HasKey(o => o.MachineName);
        });

        // Configure Configuration entity
        modelBuilder.Entity<ConnectPro.Configuration>(entity =>
        {
            entity.HasKey(c => c.ID);
            entity.Property(c => c.ID).ValueGeneratedOnAdd();
            
            entity.Property(c => c.MachineName).IsRequired();
            entity.HasIndex(c => c.MachineName).IsUnique();

            entity.Property(c => c.ServerAddr);
            entity.Property(c => c.Port);
            entity.Property(c => c.UserName);
            entity.Property(c => c.Password);
            entity.Property(c => c.Realm);
            entity.Property(c => c.OperatorDirNo);
            entity.Property(c => c.ControllerName);
            entity.Property(c => c.DisplayConfigurationInSmartClient);
            entity.Property(c => c.EnablePopupWindow);

            // Configure Operator relationship with shadow property
            entity.HasOne(c => c.Operator)
                  .WithMany()
                  .HasForeignKey("OperatorMachineName")
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Device entity
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasIndex(d => d.device_ip);
        });

        // Configure DeviceCamera relationship
        modelBuilder.Entity<DeviceCamera>(entity =>
        {
            entity.HasKey(dc => new { dc.DeviceId, dc.CameraId });

            entity.HasOne(dc => dc.Device)
                  .WithMany(d => d.DeviceCameras)
                  .HasForeignKey(dc => dc.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dc => dc.Camera)
                  .WithMany(c => c.DeviceCameras)
                  .HasForeignKey(dc => dc.CameraId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
