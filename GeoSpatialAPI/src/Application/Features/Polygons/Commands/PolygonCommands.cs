using MediatR;
using Application.Common.Models;
using Application.Features.Polygons.Queries;

namespace Application.Features.Polygons.Commands;

public record CreatePolygonCommand(string Name, string? Description, List<List<List<double>>> Coordinates) : IRequest<Result<PolygonDto>>;

public record UpdatePolygonCommand(Guid Id, string? Name, string? Description, List<List<List<double>>>? Coordinates) : IRequest<Result<PolygonDto>>;

public record DeletePolygonCommand(Guid Id) : IRequest<Result<bool>>;