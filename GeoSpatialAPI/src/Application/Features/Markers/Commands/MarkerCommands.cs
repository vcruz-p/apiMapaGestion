using MediatR;
using Application.Common.Models;
using Application.Features.Markers.Queries;
using NetTopologySuite.Geometries;

namespace Application.Features.Markers.Commands;

// IMPORTANTE: Implementar IRequest<Result<MarkerDto>>
public record CreateMarkerCommand : IRequest<Result<MarkerDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public record UpdateMarkerCommand : IRequest<Result<MarkerDto>>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public record DeleteMarkerCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}