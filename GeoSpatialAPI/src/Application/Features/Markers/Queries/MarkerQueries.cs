using MediatR;
using Application.Common.Models;

namespace Application.Features.Markers.Queries;

// IMPORTANTE: Implementar IRequest<Result<IEnumerable<MarkerDto>>>
public record GetMarkersQuery : IRequest<Result<IEnumerable<MarkerDto>>>;

public record GetMarkerByIdQuery(Guid Id) : IRequest<Result<MarkerDto>>;

public record GetNearbyMarkersQuery(double Latitude, double Longitude, double RadiusInMeters = 10000.0) : IRequest<Result<IEnumerable<MarkerDto>>>;