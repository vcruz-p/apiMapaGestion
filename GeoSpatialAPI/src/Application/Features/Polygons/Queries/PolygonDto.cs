namespace Application.Features.Polygons.Queries;

public record PolygonDto(
    Guid Id,
    string Name,
    string? Description,
    List<List<List<double>>> Coordinates,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    string UpdatedBy,
    Guid OrganizationId
);
