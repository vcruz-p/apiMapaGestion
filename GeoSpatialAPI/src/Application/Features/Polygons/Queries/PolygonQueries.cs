namespace Application.Features.Polygons.Queries;

public record GetPolygonsQuery();

public record GetPolygonByIdQuery(Guid Id);

public record PolygonDto(
    Guid Id,
    string Name,
    string? Description,
    List<List<double>> Coordinates,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CreatedBy,
    int UpdatedBy,
    int OrganizationId
);
