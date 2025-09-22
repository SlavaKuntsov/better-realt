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
	public double? AreaKitchen { get; set; } // NEW

	public List<string> Images { get; set; } = [];
	public string? ImageUrl { get; set; }

	public decimal? PriceUsd { get; set; } // 840
	public decimal? PriceRub { get; set; } // у вас было "933" в комментарии — это BYN
	public decimal? PriceByn { get; set; } // 933 (NEW)
	public decimal? PriceRubRus { get; set; } // 643 (NEW)
	public decimal? PriceEur { get; set; } // 978 (NEW)

	public List<string> ContactPhones { get; set; } = [];
	public string? ContactName { get; set; }
	public string? ContactEmail { get; set; }

	public string? Address { get; set; }
	public int? Rooms { get; set; }
	public int? Storey { get; set; }
	public int? Storeys { get; set; }

	public int? BuildingYear { get; set; } // NEW
	public int? OverhaulYear { get; set; } // NEW

	public string? Layout { get; set; } // NEW
	public string? BalconyType { get; set; } // NEW
	public string? RepairState { get; set; } // NEW
	public bool? Furniture { get; set; } // NEW
	public string? Toilet { get; set; } // NEW

	public string? Prepayment { get; set; } // NEW
	public string? HousingRent { get; set; } // NEW
	public string? LeasePeriod { get; set; } // NEW
	public List<string> Appliances { get; set; } = []; // NEW

	public double? Longitude { get; set; } // NEW  // ASSUMPTION: location[0] — lon
	public double? Latitude { get; set; } // NEW  // ASSUMPTION: location[1] — lat

	public string? TownName { get; set; } // NEW
	public string? TownDistrictName { get; set; } // NEW
	public string? TownSubDistrictName { get; set; } // NEW
	public string? StreetName { get; set; } // NEW
	public int? HouseNumber { get; set; } // NEW
	public string? BuildingNumber { get; set; } // NEW

	public string? Seller { get; set; } // NEW
	public bool? Paid { get; set; } // NEW
	public int? ViewsCount { get; set; } // NEW

	public DateTimeOffset? RaiseDate { get; set; } // NEW
	public DateTimeOffset? NewAgainDate { get; set; } // NEW

	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
}