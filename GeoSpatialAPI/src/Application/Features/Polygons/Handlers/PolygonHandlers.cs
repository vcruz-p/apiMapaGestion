using Application.Common.Models;
using Application.Features.Polygons.Commands;
using Application.Features.Polygons.Queries;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Application.Features.Polygons.Handlers;

public class CreatePolygonHandler : IRequestHandler<CreatePolygonCommand, Result<PolygonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreatePolygonHandler> _logger;

    public CreatePolygonHandler(
        IUnitOfWork unitOfWork,
        ICurrentContextService context,
        ICacheService cache,
        IEventBus eventBus,
        ILogger<CreatePolygonHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cache = cache;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<PolygonDto>> Handle(CreatePolygonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var coordinates = request.Coordinates.Select(ring => 
                ring.Select(coord => new Coordinate(coord[0], coord[1])).ToArray()
            ).ToArray();

            var linearRing = new LinearRing(coordinates[0]);
            var polygonGeometry = new NetTopologySuite.Geometries.Polygon(linearRing);

            var polygon = new Domain.Entities.Polygon
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Geometry = polygonGeometry,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _context.UserId,
                UpdatedBy = _context.UserId,
                OrganizationId = _context.OrganizationId,
                IsDeleted = false
            };

            await _unitOfWork.Polygons.AddAsync(polygon, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:polygons:*", cancellationToken);
            await _eventBus.PublishAsync("polygon.created", 
                System.Text.Json.JsonSerializer.Serialize(new { polygon.Id, polygon.OrganizationId }), 
                cancellationToken);

            var dto = new PolygonDto(
                polygon.Id, polygon.Name, polygon.Description,
                request.Coordinates,
                polygon.CreatedAt, polygon.UpdatedAt,
                polygon.CreatedBy, polygon.UpdatedBy, polygon.OrganizationId);

            return Result<PolygonDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating polygon");
            return Result<PolygonDto>.Fail("Error creating polygon");
        }
    }
}

public class GetPolygonsHandler : IRequestHandler<GetPolygonsQuery, Result<IEnumerable<PolygonDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetPolygonsHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<IEnumerable<PolygonDto>>> Handle(GetPolygonsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:polygons:all";
        
        var cached = await _cache.GetAsync<IEnumerable<PolygonDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<IEnumerable<PolygonDto>>.Ok(cached);

        var polygons = await _unitOfWork.Polygons.GetAllAsync(cancellationToken);
        var dtos = polygons.Select(p => 
        {
            var coordinatesList = new List<List<List<double>>>();
            if (p.Geometry != null)
            {
                // Exterior ring
                var exteriorRing = p.Geometry.Shell;
                var exteriorCoords = exteriorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
                coordinatesList.Add(exteriorCoords);
                
                // Interior rings (holes)
                foreach (var interiorRing in p.Geometry.Holes)
                {
                    var interiorCoords = interiorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
                    coordinatesList.Add(interiorCoords);
                }
            }
            
            return new PolygonDto(
                p.Id, p.Name, p.Description,
                coordinatesList,
                p.CreatedAt, p.UpdatedAt,
                p.CreatedBy, p.UpdatedBy, p.OrganizationId);
        });

        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5), cancellationToken);
        return Result<IEnumerable<PolygonDto>>.Ok(dtos);
    }
}

public class GetPolygonByIdHandler : IRequestHandler<GetPolygonByIdQuery, Result<PolygonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetPolygonByIdHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<PolygonDto>> Handle(GetPolygonByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:polygons:{request.Id}";
        
        var cached = await _cache.GetAsync<PolygonDto>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<PolygonDto>.Ok(cached);

        var polygon = await _unitOfWork.Polygons.GetByIdAsync(request.Id, cancellationToken);
        if (polygon == null)
            return Result<PolygonDto>.Fail("Polygon not found");

        var coordinatesList = new List<List<List<double>>>();
        if (polygon.Geometry != null)
        {
            // Exterior ring
            var exteriorRing = polygon.Geometry.Shell;
            var exteriorCoords = exteriorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
            coordinatesList.Add(exteriorCoords);
            
            // Interior rings (holes)
            foreach (var interiorRing in polygon.Geometry.Holes)
            {
                var interiorCoords = interiorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
                coordinatesList.Add(interiorCoords);
            }
        }

        var dto = new PolygonDto(
            polygon.Id, polygon.Name, polygon.Description,
            coordinatesList,
            polygon.CreatedAt, polygon.UpdatedAt,
            polygon.CreatedBy, polygon.UpdatedBy, polygon.OrganizationId);

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return Result<PolygonDto>.Ok(dto);
    }
}

public class UpdatePolygonHandler : IRequestHandler<UpdatePolygonCommand, Result<PolygonDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public UpdatePolygonHandler(
        IUnitOfWork unitOfWork,
        ICurrentContextService context,
        ICacheService cache,
        IEventBus eventBus)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cache = cache;
        _eventBus = eventBus;
    }

    public async Task<Result<PolygonDto>> Handle(UpdatePolygonCommand request, CancellationToken cancellationToken)
    {
        var polygon = await _unitOfWork.Polygons.GetByIdAsync(request.Id, cancellationToken);
        if (polygon == null)
            return Result<PolygonDto>.Fail("Polygon not found");

        var coordinates = request.Coordinates?.Select(ring => 
            ring.Select(coord => new Coordinate(coord[0], coord[1])).ToArray()
        ).ToArray();

        if (coordinates != null && coordinates.Length > 0)
        {
            var linearRing = new LinearRing(coordinates[0]);
            polygon.Geometry = new NetTopologySuite.Geometries.Polygon(linearRing);
        }
        
        polygon.Name = request.Name ?? polygon.Name;
        polygon.Description = request.Description;
        polygon.UpdatedAt = DateTime.UtcNow;
        polygon.UpdatedBy = _context.UserId;

        await _unitOfWork.Polygons.UpdateAsync(polygon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"org:{_context.OrganizationId}:polygons:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:polygons:*", cancellationToken);
        await _eventBus.PublishAsync("polygon.updated", 
            System.Text.Json.JsonSerializer.Serialize(new { polygon.Id, polygon.OrganizationId }), 
            cancellationToken);

        var coordsList = request.Coordinates ?? new List<List<List<double>>>();
        var dto = new PolygonDto(
            polygon.Id, polygon.Name ?? string.Empty, polygon.Description,
            coordsList,
            polygon.CreatedAt, polygon.UpdatedAt,
            polygon.CreatedBy, polygon.UpdatedBy, polygon.OrganizationId);

        return Result<PolygonDto>.Ok(dto);
    }
}

public class DeletePolygonHandler : IRequestHandler<DeletePolygonCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public DeletePolygonHandler(
        IUnitOfWork unitOfWork,
        ICurrentContextService context,
        ICacheService cache,
        IEventBus eventBus)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cache = cache;
        _eventBus = eventBus;
    }

    public async Task<Result<bool>> Handle(DeletePolygonCommand request, CancellationToken cancellationToken)
    {
        var polygon = await _unitOfWork.Polygons.GetByIdAsync(request.Id, cancellationToken);
        if (polygon == null)
            return Result<bool>.Fail("Polygon not found");

        polygon.IsDeleted = true;
        polygon.UpdatedAt = DateTime.UtcNow;
        polygon.UpdatedBy = _context.UserId;

        await _unitOfWork.Polygons.UpdateAsync(polygon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"org:{_context.OrganizationId}:polygons:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:polygons:*", cancellationToken);
        await _eventBus.PublishAsync("polygon.deleted", 
            System.Text.Json.JsonSerializer.Serialize(new { polygon.Id, polygon.OrganizationId }), 
            cancellationToken);

        return Result<bool>.Ok(true);
    }
}
