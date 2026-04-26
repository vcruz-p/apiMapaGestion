using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Data;

public class GeoDbContext : DbContext
{
    private readonly ICurrentContextService _contextService;

    public DbSet<Marker> Markers => Set<Marker>();
    public DbSet<Polygon> Polygons => Set<Polygon>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Target> Targets => Set<Target>();

    public GeoDbContext(DbContextOptions<GeoDbContext> options, ICurrentContextService contextService)
        : base(options)
    {
        _contextService = contextService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeoDbContext).Assembly);
        
        // Enable PostGIS
        modelBuilder.HasPostgresExtension("postgis");
        
        // Global query filter for multi-tenancy (applied to all entities with OrganizationId)
        modelBuilder.Entity<Marker>().HasQueryFilter(e => e.OrganizationId == _contextService.OrganizationId);
        modelBuilder.Entity<Polygon>().HasQueryFilter(e => e.OrganizationId == _contextService.OrganizationId);
        modelBuilder.Entity<Route>().HasQueryFilter(e => e.OrganizationId == _contextService.OrganizationId);
        modelBuilder.Entity<Target>().HasQueryFilter(e => e.OrganizationId == _contextService.OrganizationId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = _contextService.UserId;
            }
            
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            entry.Entity.UpdatedBy = _contextService.UserId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
