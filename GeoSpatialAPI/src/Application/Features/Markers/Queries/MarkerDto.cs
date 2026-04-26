namespace Application.Features.Markers.Queries;

public record MarkerDto(
    Guid Id,
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    string UpdatedBy,
    Guid OrganizationId
);
