using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Services;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _markerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkerService(IMarkerRepository markerRepository, IUnitOfWork unitOfWork)
    {
        _markerRepository = markerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Marker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _markerRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _markerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Marker> CreateAsync(string name, string? description, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var marker = new Marker
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Location = new Point(longitude, latitude) { SRID = 4326 }
        };

        await _markerRepository.AddAsync(marker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return marker;
    }

    public async Task<Marker?> UpdateAsync(Guid id, string? name, string? description, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var marker = await _markerRepository.GetByIdAsync(id, cancellationToken);
        if (marker == null) return null;

        if (name != null) marker.Name = name;
        if (description != null) marker.Description = description;
        marker.Location = new Point(longitude, latitude) { SRID = 4326 };

        await _markerRepository.UpdateAsync(marker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return marker;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var marker = await _markerRepository.GetByIdAsync(id, cancellationToken);
        if (marker == null) return false;

        await _markerRepository.DeleteAsync(marker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Marker>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default)
    {
        return await _markerRepository.GetNearbyAsync(latitude, longitude, radiusInMeters, cancellationToken);
    }
}

public class PolygonService : IPolygonService
{
    private readonly IPolygonRepository _polygonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PolygonService(IPolygonRepository polygonRepository, IUnitOfWork unitOfWork)
    {
        _polygonRepository = polygonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Entities.Polygon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _polygonRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Entities.Polygon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _polygonRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Entities.Polygon> CreateAsync(string name, string? description, List<List<List<double>>> coordinates, CancellationToken cancellationToken = default)
    {
        var polygon = new Entities.Polygon
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Geometry = CreatePolygonFromCoordinates(coordinates)
        };

        await _polygonRepository.AddAsync(polygon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return polygon;
    }

    public async Task<Entities.Polygon?> UpdateAsync(Guid id, string? name, string? description, List<List<List<double>>>? coordinates, CancellationToken cancellationToken = default)
    {
        var polygon = await _polygonRepository.GetByIdAsync(id, cancellationToken);
        if (polygon == null) return null;

        if (name != null) polygon.Name = name;
        if (description != null) polygon.Description = description;
        if (coordinates != null) polygon.Geometry = CreatePolygonFromCoordinates(coordinates);

        await _polygonRepository.UpdateAsync(polygon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return polygon;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var polygon = await _polygonRepository.GetByIdAsync(id, cancellationToken);
        if (polygon == null) return false;

        await _polygonRepository.DeleteAsync(polygon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static NetTopologySuite.Geometries.Polygon CreatePolygonFromCoordinates(List<List<List<double>>> coordinates)
    {
        if (coordinates.Count == 0 || coordinates[0].Count < 4)
            throw new ArgumentException("Invalid polygon coordinates");

        var exteriorRing = coordinates[0].Select(coord => new Coordinate(coord[0], coord[1])).ToArray();
        var factory = new GeometryFactory(new PrecisionModel(), 4326);
        return factory.CreatePolygon(exteriorRing);
    }
}

public class RouteService : IRouteService
{
    private readonly IRouteRepository _routeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RouteService(IRouteRepository routeRepository, IUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _routeRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _routeRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Route> CreateAsync(string name, string? description, LineString geometry, CancellationToken cancellationToken = default)
    {
        var route = new Route
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Geometry = geometry
        };

        await _routeRepository.AddAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return route;
    }

    public async Task<Route?> UpdateAsync(Guid id, string? name, string? description, LineString? geometry, CancellationToken cancellationToken = default)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken);
        if (route == null) return null;

        if (name != null) route.Name = name;
        if (description != null) route.Description = description;
        if (geometry != null) route.Geometry = geometry;

        await _routeRepository.UpdateAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return route;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken);
        if (route == null) return false;

        await _routeRepository.DeleteAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Route>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default)
    {
        return await _routeRepository.GetNearbyAsync(latitude, longitude, radiusInMeters, cancellationToken);
    }
}

public class TargetService : ITargetService
{
    private readonly ITargetRepository _targetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TargetService(ITargetRepository targetRepository, IUnitOfWork unitOfWork)
    {
        _targetRepository = targetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Target>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _targetRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Target?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _targetRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Target> CreateAsync(string name, string? description, Guid? parentId, string parentType, Point? location, CancellationToken cancellationToken = default)
    {
        var target = new Target
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ParentId = parentId,
            ParentType = parentType,
            Location = location
        };

        await _targetRepository.AddAsync(target, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return target;
    }

    public async Task<Target?> UpdateAsync(Guid id, string? name, string? description, Guid? parentId, string? parentType, Point? location, CancellationToken cancellationToken = default)
    {
        var target = await _targetRepository.GetByIdAsync(id, cancellationToken);
        if (target == null) return null;

        if (name != null) target.Name = name;
        if (description != null) target.Description = description;
        if (parentId.HasValue) target.ParentId = parentId.Value;
        if (parentType != null) target.ParentType = parentType;
        if (location != null) target.Location = location;

        await _targetRepository.UpdateAsync(target, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return target;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var target = await _targetRepository.GetByIdAsync(id, cancellationToken);
        if (target == null) return false;

        await _targetRepository.DeleteAsync(target, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Target>> GetByParentAsync(Guid parentId, string parentType, CancellationToken cancellationToken = default)
    {
        return await _targetRepository.GetByParentAsync(parentId, parentType, cancellationToken);
    }
}
