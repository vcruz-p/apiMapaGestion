using Application.Common.Models;
using Application.Features.AreaMapas.Commands;
using Application.Features.AreaMapas.Queries;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Application.Features.AreaMapas.Handlers;

public class CreateAreaMapaHandler : IRequestHandler<CreateAreaMapaCommand, Result<AreaMapaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateAreaMapaHandler> _logger;

    public CreateAreaMapaHandler(
        IUnitOfWork unitOfWork,
        ICurrentContextService context,
        ICacheService cache,
        IEventBus eventBus,
        ILogger<CreateAreaMapaHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cache = cache;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<AreaMapaDto>> Handle(CreateAreaMapaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var coordinates = request.Coordinates.Select(ring => 
                ring.Select(coord => new Coordinate(coord[0], coord[1])).ToArray()
            ).ToArray();

            var linearRing = new LinearRing(coordinates[0]);
            var areaMapaGeometry = new NetTopologySuite.Geometries.Polygon(linearRing);

            var areaMapa = new AreaMapa
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Geometry = areaMapaGeometry,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _context.UserId,
                UpdatedBy = _context.UserId,
                OrganizationId = _context.OrganizationId,
                IsDeleted = false
            };

            await _unitOfWork.AreaMapas.AddAsync(areaMapa, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:areamapas:*", cancellationToken);
            await _eventBus.PublishAsync("areamapa.created", 
                System.Text.Json.JsonSerializer.Serialize(new { areaMapa.Id, areaMapa.OrganizationId }), 
                cancellationToken);

            var dto = new AreaMapaDto(
                areaMapa.Id, areaMapa.Name, areaMapa.Description,
                request.Coordinates,
                areaMapa.CreatedAt, areaMapa.UpdatedAt,
                areaMapa.CreatedBy, areaMapa.UpdatedBy, areaMapa.OrganizationId);

            return Result<AreaMapaDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating area mapa");
            return Result<AreaMapaDto>.Fail("Error creating area mapa");
        }
    }
}

public class GetAreaMapasHandler : IRequestHandler<GetAreaMapasQuery, Result<IEnumerable<AreaMapaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetAreaMapasHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<IEnumerable<AreaMapaDto>>> Handle(GetAreaMapasQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:areamapas:all";
        
        var cached = await _cache.GetAsync<IEnumerable<AreaMapaDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<IEnumerable<AreaMapaDto>>.Ok(cached);

        var areaMapas = await _unitOfWork.AreaMapas.GetAllAsync(cancellationToken);
        var dtos = areaMapas.Select(p => 
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
            
            return new AreaMapaDto(
                p.Id, p.Name, p.Description,
                coordinatesList,
                p.CreatedAt, p.UpdatedAt,
                p.CreatedBy, p.UpdatedBy, p.OrganizationId);
        });

        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5), cancellationToken);
        return Result<IEnumerable<AreaMapaDto>>.Ok(dtos);
    }
}

public class GetAreaMapaByIdHandler : IRequestHandler<GetAreaMapaByIdQuery, Result<AreaMapaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetAreaMapaByIdHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<AreaMapaDto>> Handle(GetAreaMapaByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:areamapas:{request.Id}";
        
        var cached = await _cache.GetAsync<AreaMapaDto>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<AreaMapaDto>.Ok(cached);

        var areaMapa = await _unitOfWork.AreaMapas.GetByIdAsync(request.Id, cancellationToken);
        if (areaMapa == null)
            return Result<AreaMapaDto>.Fail("AreaMapa not found");

        var coordinatesList = new List<List<List<double>>>();
        if (areaMapa.Geometry != null)
        {
            // Exterior ring
            var exteriorRing = areaMapa.Geometry.Shell;
            var exteriorCoords = exteriorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
            coordinatesList.Add(exteriorCoords);
            
            // Interior rings (holes)
            foreach (var interiorRing in areaMapa.Geometry.Holes)
            {
                var interiorCoords = interiorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList();
                coordinatesList.Add(interiorCoords);
            }
        }

        var dto = new AreaMapaDto(
            areaMapa.Id, areaMapa.Name, areaMapa.Description,
            coordinatesList,
            areaMapa.CreatedAt, areaMapa.UpdatedAt,
            areaMapa.CreatedBy, areaMapa.UpdatedBy, areaMapa.OrganizationId);

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return Result<AreaMapaDto>.Ok(dto);
    }
}

public class UpdateAreaMapaHandler : IRequestHandler<UpdateAreaMapaCommand, Result<AreaMapaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public UpdateAreaMapaHandler(
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

    public async Task<Result<AreaMapaDto>> Handle(UpdateAreaMapaCommand request, CancellationToken cancellationToken)
    {
        var areaMapa = await _unitOfWork.AreaMapas.GetByIdAsync(request.Id, cancellationToken);
        if (areaMapa == null)
            return Result<AreaMapaDto>.Fail("AreaMapa not found");

        var coordinates = request.Coordinates?.Select(ring => 
            ring.Select(coord => new Coordinate(coord[0], coord[1])).ToArray()
        ).ToArray();

        if (coordinates != null && coordinates.Length > 0)
        {
            var linearRing = new LinearRing(coordinates[0]);
            areaMapa.Geometry = new NetTopologySuite.Geometries.Polygon(linearRing);
        }
        
        areaMapa.Name = request.Name ?? areaMapa.Name;
        areaMapa.Description = request.Description;
        areaMapa.UpdatedAt = DateTime.UtcNow;
        areaMapa.UpdatedBy = _context.UserId;

        await _unitOfWork.AreaMapas.UpdateAsync(areaMapa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"org:{_context.OrganizationId}:areamapas:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:areamapas:*", cancellationToken);
        await _eventBus.PublishAsync("areamapa.updated", 
            System.Text.Json.JsonSerializer.Serialize(new { areaMapa.Id, areaMapa.OrganizationId }), 
            cancellationToken);

        var coordsList = request.Coordinates ?? new List<List<List<double>>>();
        var dto = new AreaMapaDto(
            areaMapa.Id, areaMapa.Name ?? string.Empty, areaMapa.Description,
            coordsList,
            areaMapa.CreatedAt, areaMapa.UpdatedAt,
            areaMapa.CreatedBy, areaMapa.UpdatedBy, areaMapa.OrganizationId);

        return Result<AreaMapaDto>.Ok(dto);
    }
}

public class DeleteAreaMapaHandler : IRequestHandler<DeleteAreaMapaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public DeleteAreaMapaHandler(
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

    public async Task<Result<bool>> Handle(DeleteAreaMapaCommand request, CancellationToken cancellationToken)
    {
        var areaMapa = await _unitOfWork.AreaMapas.GetByIdAsync(request.Id, cancellationToken);
        if (areaMapa == null)
            return Result<bool>.Fail("AreaMapa not found");

        areaMapa.IsDeleted = true;
        areaMapa.UpdatedAt = DateTime.UtcNow;
        areaMapa.UpdatedBy = _context.UserId;

        await _unitOfWork.AreaMapas.UpdateAsync(areaMapa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"org:{_context.OrganizationId}:areamapas:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:areamapas:*", cancellationToken);
        await _eventBus.PublishAsync("areamapa.deleted", 
            System.Text.Json.JsonSerializer.Serialize(new { areaMapa.Id, areaMapa.OrganizationId }), 
            cancellationToken);

        return Result<bool>.Ok(true);
    }
}
