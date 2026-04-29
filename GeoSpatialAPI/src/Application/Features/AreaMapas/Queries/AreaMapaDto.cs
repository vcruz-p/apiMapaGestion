namespace Application.Features.AreaMapas.Queries;

public record AreaMapaDto(
    Guid Id,
    string Name,
    string? Description,
    List<List<List<double>>> Coordinates,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CreatedBy,
    int UpdatedBy,
    int OrganizationId
);
