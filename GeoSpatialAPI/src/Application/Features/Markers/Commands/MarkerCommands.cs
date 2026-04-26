namespace Application.Features.Markers.Commands;

public record CreateMarkerCommand(
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    Dictionary<string, object>? Metadata = null
);

public record UpdateMarkerCommand(
    Guid Id,
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    Dictionary<string, object>? Metadata = null
);

public record DeleteMarkerCommand(Guid Id);
