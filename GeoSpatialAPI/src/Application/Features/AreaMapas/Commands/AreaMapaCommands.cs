using MediatR;
using Application.Common.Models;
using Application.Features.AreaMapas.Queries;

namespace Application.Features.AreaMapas.Commands;

public record CreateAreaMapaCommand(string Name, string? Description, List<List<List<double>>> Coordinates) : IRequest<Result<AreaMapaDto>>;

public record UpdateAreaMapaCommand(Guid Id, string? Name, string? Description, List<List<List<double>>>? Coordinates) : IRequest<Result<AreaMapaDto>>;

public record DeleteAreaMapaCommand(Guid Id) : IRequest<Result<bool>>;