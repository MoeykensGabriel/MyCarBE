using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Data.Configurations;
using MyCarBE.Data.Identity;
using MyCarBE.Domain.Common;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Domain DbSets
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Fleet> Fleets => Set<Fleet>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderStatusChange> WorkOrderStatusChanges => Set<WorkOrderStatusChange>();
    public DbSet<WorkOrderService> WorkOrderServices => Set<WorkOrderService>();
    public DbSet<WorkOrderPhoto> WorkOrderPhotos => Set<WorkOrderPhoto>();
    public DbSet<CatalogService> CatalogServices => Set<CatalogService>();
    public DbSet<MaintenanceAlert> MaintenanceAlerts => Set<MaintenanceAlert>();
    public DbSet<DeclaredServiceHistory> DeclaredServiceHistories => Set<DeclaredServiceHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerConfiguration).Assembly);

        // Global query filter de soft delete para todas las entidades que heredan BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property  = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter    = Expression.Lambda(Expression.Not(property), parameter);

            entityType.SetQueryFilter(filter);
        }

        // WorkOrderStatusChange no hereda BaseEntity (evento inmutable sin soft delete),
        // pero su WorkOrder sí tiene query filter. Agregamos un filtro matching para
        // evitar resultados inesperados cuando se accede a StatusChanges directamente.
        modelBuilder.Entity<WorkOrderStatusChange>()
            .HasQueryFilter(s => !s.WorkOrder.IsDeleted);

        // Seed de roles
        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Admin",    NormalizedName = "ADMIN" },
            new ApplicationRole { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Customer", NormalizedName = "CUSTOMER" }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id        = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Interceptar deletes físicos y convertirlos en soft delete
                    entry.State            = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
