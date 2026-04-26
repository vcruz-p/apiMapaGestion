using MediatR;
using Application.Common.Models;

namespace Application.Features.Polygons.Queries;

public record GetPolygonsQuery : IRequest<Result<IEnumerable<PolygonDto>>>;

public record GetPolygonByIdQuery(Guid Id) : IRequest<Result<PolygonDto>>;