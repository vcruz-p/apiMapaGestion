using MediatR;
using Application.Common.Models;
using Application.Features.Markers.Queries;
using NetTopologySuite.Geometries;

namespace Application.Features.Markers.Commands;

// IMPORTANTE: Implementar IRequest<Result<MarkerDto>>
public record CreateMarkerCommand(string Name, string? Description, double Latitude, double Longitude) : IRequest<Result<MarkerDto>>;

public record UpdateMarkerCommand(Guid Id, string? Name, string? Description, double Latitude, double Longitude) : IRequest<Result<MarkerDto>>;

public record DeleteMarkerCommand(Guid Id) : IRequest<Result<bool>>;