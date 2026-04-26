using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MarkerConfiguration : IEntityTypeConfiguration<Marker>
{
    public void Configure(EntityTypeBuilder<Marker> builder)
    {
        builder.ToTable("markers");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.Geometry).HasColumnName("geometry")
            .HasColumnType("geometry(Point, 4326)")
            .HasSpatialIndex(new SpatialIndexAttribute { IsGeography = false });
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        // Indexes
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_markers_organization_id");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_markers_is_deleted");
        
        // Global filters
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasQueryFilter(e => true); // OrganizationId filter applied in DbContext
    }
}

public class PolygonConfiguration : IEntityTypeConfiguration<Domain.Entities.Polygon>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Polygon> builder)
    {
        builder.ToTable("polygons");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.Geometry).HasColumnName("geometry")
            .HasColumnType("geometry(Polygon, 4326)")
            .HasSpatialIndex(new SpatialIndexAttribute { IsGeography = false });
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_polygons_organization_id");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_polygons_is_deleted");
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.Geometry).HasColumnName("geometry")
            .HasColumnType("geometry(LineString, 4326)")
            .HasSpatialIndex(new SpatialIndexAttribute { IsGeography = false });
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_routes_organization_id");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_routes_is_deleted");
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class TargetConfiguration : IEntityTypeConfiguration<Target>
{
    public void Configure(EntityTypeBuilder<Target> builder)
    {
        builder.ToTable("targets");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.Geometry).HasColumnName("geometry")
            .HasColumnType("geometry(Point, 4326)")
            .HasSpatialIndex(new SpatialIndexAttribute { IsGeography = false });
        builder.Property(e => e.Metadata).HasColumnName("metadata")
            .HasColumnType("jsonb");
        builder.Property(e => e.ParentId).HasColumnName("parent_id");
        builder.Property(e => e.ParentType).HasColumnName("parent_type").HasMaxLength(50);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_targets_organization_id");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_targets_is_deleted");
        builder.HasIndex(e => e.ParentId).HasDatabaseName("ix_targets_parent_id");
        
        // GIN index for JSONB
        builder.HasIndex(e => e.Metadata)
            .HasDatabaseName("ix_targets_metadata_gin")
            .HasMethod("GIN");
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
