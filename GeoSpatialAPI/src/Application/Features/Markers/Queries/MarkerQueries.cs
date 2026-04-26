namespace Application.Features.Markers.Queries;

public record GetMarkersQuery();

public record GetMarkerByIdQuery(Guid Id);

public record GetNearbyMarkersQuery(double Latitude, double Longitude, double RadiusInMeters);

public record MarkerDto(
    Guid Id,
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CreatedBy,
    int UpdatedBy,
    int OrganizationId,
    Dictionary<string, object>? Metadata = null
);
