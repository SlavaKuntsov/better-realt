namespace Flatly.Core.RealEstate;

public class RealEstateModel
{
	public Guid Id { get; set; }

	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Headline { get; set; }

	public int? Code { get; set; }

	public double? AreaTotal { get; set; }
	public double? AreaLiving { get; set; }

	public List<string> Images { get; set; } = [];
	public string? ImageUrl { get; set; }

	public decimal? PriceUsd { get; set; }  // 840
	public decimal? PriceRub { get; set; }  // 933

	public List<string> ContactPhones { get; set; } = [];
	public string? ContactName { get; set; }
	public string? ContactEmail { get; set; }

	public string? Address { get; set; }
	public int? Rooms { get; set; }
	public int? Storey { get; set; }
	public int? Storeys { get; set; }

	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
}