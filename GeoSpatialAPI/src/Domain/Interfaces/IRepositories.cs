namespace Domain.Interfaces;

using Domain.Entities;

public interface ICurrentContextService
{
    int UserId { get; }
    int OrganizationId { get; }
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default);
    Task SubscribeAsync(string channel, Func<string, Task> handler, CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

public interface IMarkerRepository : IRepository<Domain.Entities.Marker>
{
    Task<IEnumerable<Domain.Entities.Marker>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Marker>> GetIntersectingAsync(NetTopologySuite.Geometries.Geometry geometry, CancellationToken cancellationToken = default);
}

public interface IAreaMapaRepository : IRepository<AreaMapa>
{
    Task<IEnumerable<AreaMapa>> GetIntersectingAsync(NetTopologySuite.Geometries.Geometry geometry, CancellationToken cancellationToken = default);
}

public interface IRouteRepository : IRepository<Domain.Entities.Route>
{
    Task<IEnumerable<Domain.Entities.Route>> GetNearbyAsync(double latitude, double longitude, double radiusInMeters, CancellationToken cancellationToken = default);
}

public interface ITargetRepository : IRepository<Domain.Entities.Target>
{
    Task<IEnumerable<Domain.Entities.Target>> GetByParentAsync(Guid parentId, string parentType, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    IMarkerRepository Markers { get; }
    IAreaMapaRepository AreaMapas { get; }
    IRouteRepository Routes { get; }
    ITargetRepository Targets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
