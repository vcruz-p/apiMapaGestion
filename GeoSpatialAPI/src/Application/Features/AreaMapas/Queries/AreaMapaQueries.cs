using MediatR;
using Application.Common.Models;

namespace Application.Features.AreaMapas.Queries;

public record GetAreaMapasQuery : IRequest<Result<IEnumerable<AreaMapaDto>>>;

public record GetAreaMapaByIdQuery(Guid Id) : IRequest<Result<AreaMapaDto>>;