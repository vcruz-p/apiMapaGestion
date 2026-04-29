using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Services;

public class MarkerRepository : IMarkerRepository
{
    private readonly GeoDbContext _context;

    public MarkerRepository(GeoDbContext context)
    {
        _context = context;
    }

    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Markers.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Marker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Markers.ToListAsync(cancellationToken);
    }

    public async Task<Marker> AddAsync(Marker entity, CancellationToken cancellationToken = default)
    {
        await _context.Markers.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Marker entity, CancellationToken cancellationToken = default)
    {
        _context.Markers.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Marker entity, CancellationToken cancellationToken = default)
    {
        _context.Markers.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Marker>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default)
    {
        var point = new Point(longitude, latitude) { SRID = 4326 };
        
        // Use ST_DWithin for proximity search
        var markers = await _context.Markers
            .FromSqlRaw(
                @"SELECT * FROM markers 
                  WHERE ST_DWithin(geometry, ST_GeomFromText({0}, 4326), {1})
                  ORDER BY ST_Distance(geometry, ST_GeomFromText({0}, 4326))",
                $"POINT({longitude} {latitude})", radiusInMeters)
            .ToListAsync(cancellationToken);
            
        return markers;
    }

    public async Task<IEnumerable<Marker>> GetIntersectingAsync(Geometry geometry, CancellationToken cancellationToken = default)
    {
        var wkt = geometry.AsText();
        var markers = await _context.Markers
            .FromSqlRaw(
                @"SELECT * FROM markers 
                  WHERE ST_Intersects(geometry, ST_GeomFromText({0}, 4326))",
                wkt)
            .ToListAsync(cancellationToken);
            
        return markers;
    }
}

public class AreaMapaRepository : IAreaMapaRepository
{
    private readonly GeoDbContext _context;

    public AreaMapaRepository(GeoDbContext context)
    {
        _context = context;
    }

    public async Task<AreaMapa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AreaMapas.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<AreaMapa>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AreaMapas.ToListAsync(cancellationToken);
    }

    public async Task<AreaMapa> AddAsync(AreaMapa entity, CancellationToken cancellationToken = default)
    {
        await _context.AreaMapas.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(AreaMapa entity, CancellationToken cancellationToken = default)
    {
        _context.AreaMapas.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AreaMapa entity, CancellationToken cancellationToken = default)
    {
        _context.AreaMapas.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<AreaMapa>> GetIntersectingAsync(Geometry geometry, CancellationToken cancellationToken = default)
    {
        var wkt = geometry.AsText();
        var areaMapas = await _context.AreaMapas
            .FromSqlRaw(
                @"SELECT * FROM areamapas 
                  WHERE ST_Intersects(geometry, ST_GeomFromText({0}, 4326))",
                wkt)
            .ToListAsync(cancellationToken);
            
        return areaMapas;
    }
}

public class RouteRepository : IRouteRepository
{
    private readonly GeoDbContext _context;

    public RouteRepository(GeoDbContext context)
    {
        _context = context;
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Routes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Routes.ToListAsync(cancellationToken);
    }

    public async Task<Route> AddAsync(Route entity, CancellationToken cancellationToken = default)
    {
        await _context.Routes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Route entity, CancellationToken cancellationToken = default)
    {
        _context.Routes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Route entity, CancellationToken cancellationToken = default)
    {
        _context.Routes.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Route>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default)
    {
        var point = new Point(longitude, latitude) { SRID = 4326 };
        
        var routes = await _context.Routes
            .FromSqlRaw(
                @"SELECT * FROM routes 
                  WHERE ST_DWithin(geometry, ST_GeomFromText({0}, 4326), {1})
                  ORDER BY ST_Distance(geometry, ST_GeomFromText({0}, 4326))",
                $"POINT({longitude} {latitude})", radiusInMeters)
            .ToListAsync(cancellationToken);
            
        return routes;
    }
}

public class TargetRepository : ITargetRepository
{
    private readonly GeoDbContext _context;

    public TargetRepository(GeoDbContext context)
    {
        _context = context;
    }

    public async Task<Target?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Targets.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Target>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Targets.ToListAsync(cancellationToken);
    }

    public async Task<Target> AddAsync(Target entity, CancellationToken cancellationToken = default)
    {
        await _context.Targets.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Target entity, CancellationToken cancellationToken = default)
    {
        _context.Targets.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Target entity, CancellationToken cancellationToken = default)
    {
        _context.Targets.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Target>> GetByParentAsync(Guid parentId, string parentType, CancellationToken cancellationToken = default)
    {
        return await _context.Targets
            .Where(t => t.ParentId == parentId && t.ParentType == parentType)
            .ToListAsync(cancellationToken);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly GeoDbContext _context;
    private bool _disposed;

    public IMarkerRepository Markers { get; }
    public IAreaMapaRepository AreaMapas { get; }
    public IRouteRepository Routes { get; }
    public ITargetRepository Targets { get; }

    public UnitOfWork(GeoDbContext context)
    {
        _context = context;
        Markers = new MarkerRepository(context);
        AreaMapas = new AreaMapaRepository(context);
        Routes = new RouteRepository(context);
        Targets = new TargetRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
