using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Domain.Interfaces;

public interface IMarkerService
{
    Task<IEnumerable<Marker>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Marker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Marker> CreateAsync(string name, string? description, double latitude, double longitude, CancellationToken cancellationToken = default);
    Task<Marker?> UpdateAsync(Guid id, string? name, string? description, double latitude, double longitude, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Marker>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default);
}

public interface IPolygonService
{
    Task<IEnumerable<Entities.Polygon>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Entities.Polygon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Polygon> CreateAsync(string name, string? description, List<List<List<double>>> coordinates, CancellationToken cancellationToken = default);
    Task<Entities.Polygon?> UpdateAsync(Guid id, string? name, string? description, List<List<List<double>>>? coordinates, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRouteService
{
    Task<IEnumerable<Route>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Route> CreateAsync(string name, string? description, LineString geometry, CancellationToken cancellationToken = default);
    Task<Route?> UpdateAsync(Guid id, string? name, string? description, LineString? geometry, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Route>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default);
}

public interface ITargetService
{
    Task<IEnumerable<Target>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Target?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Target> CreateAsync(string name, string? description, Guid? parentId, string parentType, Point? location, CancellationToken cancellationToken = default);
    Task<Target?> UpdateAsync(Guid id, string? name, string? description, Guid? parentId, string? parentType, Point? location, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Target>> GetByParentAsync(Guid parentId, string parentType, CancellationToken cancellationToken = default);
}
