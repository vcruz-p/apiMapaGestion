using MediatR;
using Application.Common.Models;

namespace Application.Features.Polygons.Commands;

public record CreatePolygonCommand : IRequest<Result<PolygonDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    // Lista de anillos: El primero es el exterior, los siguientes son agujeros
    // Cada anillo es una lista de coordenadas [Lon, Lat]
    public List<List<List<double>>> Coordinates { get; init; } = new();
}

public record UpdatePolygonCommand : IRequest<Result<PolygonDto>>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<List<List<double>>>? Coordinates { get; init; }
}

public record DeletePolygonCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
}