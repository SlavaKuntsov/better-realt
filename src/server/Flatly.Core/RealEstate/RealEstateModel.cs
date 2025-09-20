namespace Flatly.Core.RealEstate;

public class RealEstateModel
{
	public Guid Id { get; set; }
	public string? Title { get; set; } = string.Empty;
	public string? Description { get; set; } = string.Empty;
	public double? AreaTotal { get; set; }
	public string? ImageUrl { get; set; } = string.Empty;

	public RealEstateModel()
	{
	}
}