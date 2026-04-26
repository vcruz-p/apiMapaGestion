using MediatR;
using Application.Common.Models;

namespace Application.Features.Markers.Queries;

// IMPORTANTE: Implementar IRequest<Result<IEnumerable<MarkerDto>>>
public record GetMarkersQuery : IRequest<Result<IEnumerable<MarkerDto>>>;

public record GetMarkerByIdQuery : IRequest<Result<MarkerDto>>
{
    public Guid Id { get; init; }
}

public record GetNearbyMarkersQuery : IRequest<Result<IEnumerable<MarkerDto>>>
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double RadiusKm { get; init; } = 10.0;
}