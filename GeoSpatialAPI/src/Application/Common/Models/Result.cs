namespace Application.Common.Models;

public record GeoPoint(double Latitude, double Longitude);

public record GeoResult<T>(IEnumerable<T> Items, int TotalCount);

public record Result<T>(T? Data, bool Success, string? ErrorMessage = null)
{
    public static Result<T> Ok(T data) => new(data, true);
    public static Result<T> Fail(string error) => new(default, false, error);
}
