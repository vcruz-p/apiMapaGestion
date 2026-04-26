namespace Application.Features.Polygons.Commands;

public record CreatePolygonCommand(
    string Name,
    string? Description,
    List<List<double>> Coordinates
);

public record UpdatePolygonCommand(
    Guid Id,
    string Name,
    string? Description,
    List<List<double>> Coordinates
);

public record DeletePolygonCommand(Guid Id);
