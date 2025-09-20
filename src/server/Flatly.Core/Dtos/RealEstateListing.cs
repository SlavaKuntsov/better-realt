namespace Flatly.Core.Dtos;

public sealed record RealEstateListing(
    string Title,
    string? Description,
    double? AreaTotal,
    string? ImageUrl);
