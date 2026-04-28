using Application.Common.Models;
using Application.Features.Markers.Commands;
using Application.Features.Markers.Queries;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Markers.Handlers;

public class CreateMarkerHandler : IRequestHandler<CreateMarkerCommand, Result<MarkerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateMarkerHandler> _logger;

    public CreateMarkerHandler(
        IUnitOfWork unitOfWork,
        ICurrentContextService context,
        ICacheService cache,
        IEventBus eventBus,
        ILogger<CreateMarkerHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cache = cache;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<MarkerDto>> Handle(CreateMarkerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var marker = new Marker
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Geometry = new NetTopologySuite.Geometries.Point(request.Longitude, request.Latitude),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _context.UserId,
                UpdatedBy = _context.UserId,
                OrganizationId = _context.OrganizationId,
                IsDeleted = false
            };

            await _unitOfWork.Markers.AddAsync(marker, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:markers:*", cancellationToken);

            // Publish event
            await _eventBus.PublishAsync("marker.created", 
                System.Text.Json.JsonSerializer.Serialize(new { marker.Id, marker.OrganizationId }), 
                cancellationToken);

            var dto = new MarkerDto(
                marker.Id, marker.Name, marker.Description,
                marker.Geometry.Y, marker.Geometry.X,
                marker.CreatedAt, marker.UpdatedAt,
                marker.CreatedBy, marker.UpdatedBy, marker.OrganizationId);

            return Result<MarkerDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marker");
            return Result<MarkerDto>.Fail("Error creating marker");
        }
    }
}

public class GetMarkersHandler : IRequestHandler<GetMarkersQuery, Result<IEnumerable<MarkerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;
    private readonly ILogger<GetMarkersHandler> _logger;

    public GetMarkersHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context,
        ILogger<GetMarkersHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<MarkerDto>>> Handle(GetMarkersQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:markers:all";
        
        var cached = await _cache.GetAsync<IEnumerable<MarkerDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<IEnumerable<MarkerDto>>.Ok(cached);

        var markers = await _unitOfWork.Markers.GetAllAsync(cancellationToken);
        var dtos = markers.Select(m => new MarkerDto(
            m.Id, m.Name, m.Description,
            m.Geometry?.Y ?? 0, m.Geometry?.X ?? 0,
            m.CreatedAt, m.UpdatedAt,
            m.CreatedBy, m.UpdatedBy, m.OrganizationId));

        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<IEnumerable<MarkerDto>>.Ok(dtos);
    }
}

public class GetMarkerByIdHandler : IRequestHandler<GetMarkerByIdQuery, Result<MarkerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetMarkerByIdHandler(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentContextService context)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<MarkerDto>> Handle(GetMarkerByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:markers:{request.Id}";
        
        var cached = await _cache.GetAsync<MarkerDto>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<MarkerDto>.Ok(cached);

        var marker = await _unitOfWork.Markers.GetByIdAsync(request.Id, cancellationToken);
        if (marker == null)
            return Result<MarkerDto>.Fail("Marker not found");

        var dto = new MarkerDto(
            marker.Id, marker.Name, marker.Description,
            marker.Geometry?.Y ?? 0, marker.Geometry?.X ?? 0,
            marker.CreatedAt, marker.UpdatedAt,
            marker.CreatedBy, marker.UpdatedBy, marker.OrganizationId);

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<MarkerDto>.Ok(dto);
    }
}

public class UpdateMarkerHandler : IRequestHandler<UpdateMarkerCommand, Result<MarkerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public UpdateMarkerHandler(
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

    public async Task<Result<MarkerDto>> Handle(UpdateMarkerCommand request, CancellationToken cancellationToken)
    {
        var marker = await _unitOfWork.Markers.GetByIdAsync(request.Id, cancellationToken);
        if (marker == null)
            return Result<MarkerDto>.Fail("Marker not found");

        marker.Name = request.Name ?? marker.Name;
        marker.Description = request.Description;
        marker.Geometry = new NetTopologySuite.Geometries.Point(request.Longitude, request.Latitude);
        marker.UpdatedAt = DateTime.UtcNow;
        marker.UpdatedBy = _context.UserId;

        await _unitOfWork.Markers.UpdateAsync(marker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"org:{_context.OrganizationId}:markers:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:markers:*", cancellationToken);

        // Publish event
        await _eventBus.PublishAsync("marker.updated", 
            System.Text.Json.JsonSerializer.Serialize(new { marker.Id, marker.OrganizationId }), 
            cancellationToken);

        var dto = new MarkerDto(
            marker.Id, marker.Name ?? string.Empty, marker.Description,
            marker.Geometry.Y, marker.Geometry.X,
            marker.CreatedAt, marker.UpdatedAt,
            marker.CreatedBy, marker.UpdatedBy, marker.OrganizationId);

        return Result<MarkerDto>.Ok(dto);
    }
}

public class DeleteMarkerHandler : IRequestHandler<DeleteMarkerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentContextService _context;
    private readonly ICacheService _cache;
    private readonly IEventBus _eventBus;

    public DeleteMarkerHandler(
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

    public async Task<Result<bool>> Handle(DeleteMarkerCommand request, CancellationToken cancellationToken)
    {
        var marker = await _unitOfWork.Markers.GetByIdAsync(request.Id, cancellationToken);
        if (marker == null)
            return Result<bool>.Fail("Marker not found");

        marker.IsDeleted = true;
        marker.UpdatedAt = DateTime.UtcNow;
        marker.UpdatedBy = _context.UserId;

        await _unitOfWork.Markers.UpdateAsync(marker, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"org:{_context.OrganizationId}:markers:{request.Id}", cancellationToken);
        await _cache.RemoveByPatternAsync($"org:{_context.OrganizationId}:markers:*", cancellationToken);

        // Publish event
        await _eventBus.PublishAsync("marker.deleted", 
            System.Text.Json.JsonSerializer.Serialize(new { marker.Id, marker.OrganizationId }), 
            cancellationToken);

        return Result<bool>.Ok(true);
    }
}

public class GetNearbyMarkersHandler : IRequestHandler<GetNearbyMarkersQuery, Result<IEnumerable<MarkerDto>>>
{
    private readonly IMarkerRepository _markerRepository;
    private readonly ICacheService _cache;
    private readonly ICurrentContextService _context;

    public GetNearbyMarkersHandler(
        IMarkerRepository markerRepository,
        ICacheService cache,
        ICurrentContextService context)
    {
        _markerRepository = markerRepository;
        _cache = cache;
        _context = context;
    }

    public async Task<Result<IEnumerable<MarkerDto>>> Handle(GetNearbyMarkersQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"org:{_context.OrganizationId}:geo:near:{request.Latitude}:{request.Longitude}:{request.RadiusInMeters}";
        
        var cached = await _cache.GetAsync<IEnumerable<MarkerDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return Result<IEnumerable<MarkerDto>>.Ok(cached);

        var markers = await _markerRepository.GetNearbyAsync(
            request.Latitude, request.Longitude, request.RadiusInMeters, cancellationToken);

        var dtos = markers.Select(m => new MarkerDto(
            m.Id, m.Name, m.Description,
            m.Geometry?.Y ?? 0, m.Geometry?.X ?? 0,
            m.CreatedAt, m.UpdatedAt,
            m.CreatedBy, m.UpdatedBy, m.OrganizationId));

        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(2), cancellationToken);

        return Result<IEnumerable<MarkerDto>>.Ok(dtos);
    }
}
