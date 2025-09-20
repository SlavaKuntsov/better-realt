using System.Globalization;
using System.Text.Json;
using Flatly.Core.Abstractions.Services;
using Flatly.Core.Dtos;
using Flatly.Core.RealEstate;
using Microsoft.Extensions.Logging;

namespace Flatly.Persistance.Services;

public sealed class RealtNextDataParser(ILogger<RealtNextDataParser> logger) : IRealtNextDataParser
{
	private const string ScriptMarker = "<script id=\"__NEXT_DATA__\"";
	private const string ScriptEndMarker = "</script>";

	public IList<RealEstateModel> Parse(string html)
	{
		var jsonPayload = ExtractNextDataJson(html);
		if (string.IsNullOrWhiteSpace(jsonPayload))
		{
			logger.LogWarning("Unable to find __NEXT_DATA__ script in the downloaded HTML.");
			return [];
		}

		try
		{
			using var document = JsonDocument.Parse(jsonPayload);
			if (!TryGetObjects(document.RootElement, out var objectsElement))
			{
				logger.LogWarning("Unable to navigate to objects collection in the JSON payload.");
				return [];
			}

			var listings = new List<RealEstateModel>();
			foreach (var item in objectsElement.EnumerateArray())
			{
				if (TryParseListing(item, out var listing))
					listings.Add(listing);
			}

			return listings;
		}
		catch (JsonException ex)
		{
			logger.LogError(ex, "Failed to parse JSON payload from __NEXT_DATA__ script.");
			return [];
		}
	}

	public PaginationInfoDto? ParsePagination(string html)
	{
		var jsonPayload = ExtractNextDataJson(html);
		if (string.IsNullOrWhiteSpace(jsonPayload))
			return null;

		try
		{
			using var document = JsonDocument.Parse(jsonPayload);
			if (!TryGetPagination(document.RootElement, out var page, out var pageSize, out var total))
				return null;

			return new PaginationInfoDto(page, pageSize, total);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	private static string? ExtractNextDataJson(string html)
	{
		var scriptStartIndex = html.IndexOf(ScriptMarker, StringComparison.OrdinalIgnoreCase);
		if (scriptStartIndex < 0) return null;

		var contentStartIndex = html.IndexOf('>', scriptStartIndex);
		if (contentStartIndex < 0) return null;

		contentStartIndex += 1;
		if (contentStartIndex >= html.Length) return null;

		var scriptEndIndex = html.IndexOf(ScriptEndMarker, contentStartIndex, StringComparison.OrdinalIgnoreCase);
		if (scriptEndIndex < 0 || scriptEndIndex <= contentStartIndex) return null;

		return html.Substring(contentStartIndex, scriptEndIndex - contentStartIndex);
	}

	private static bool TryGetObjects(JsonElement root, out JsonElement objectsElement)
	{
		objectsElement = default;
		if (!root.TryGetProperty("props", out var props) ||
		    !props.TryGetProperty("pageProps", out var pageProps) ||
		    !pageProps.TryGetProperty("initialState", out var initialState) ||
		    !initialState.TryGetProperty("objectsListing", out var objectsListing) ||
		    !objectsListing.TryGetProperty("objects", out var objects) ||
		    objects.ValueKind != JsonValueKind.Array)
			return false;

		objectsElement = objects;
		return true;
	}

	private static bool TryGetPagination(JsonElement root, out int page, out int pageSize, out int totalCount)
	{
		page = 1;
		pageSize = 0;
		totalCount = 0;

		if (!root.TryGetProperty("props", out var props) ||
		    !props.TryGetProperty("pageProps", out var pageProps) ||
		    !pageProps.TryGetProperty("initialState", out var initialState) ||
		    !initialState.TryGetProperty("objectsListing", out var objectsListing))
			return false;

		if (objectsListing.TryGetProperty("pagination", out var pagination))
		{
			page = GetInt(pagination, "page") ?? 1;
			pageSize = GetInt(pagination, "pageSize") ?? 0;
			totalCount = GetInt(pagination, "totalCount") ?? 0;
			return true;
		}

		return false;
	}

	private static bool TryParseListing(JsonElement element, out RealEstateModel listing)
	{
		var title = NormalizeString(GetString(element, "title")) ?? string.Empty;
		var description = NormalizeString(GetString(element, "description")) ?? string.Empty;
		var headline = NormalizeString(GetString(element, "headline")) ?? string.Empty;

		var code = GetInt(element, "code");
		var areaTotal = GetDouble(element, "areaTotal");
		var areaLiving = GetDouble(element, "areaLiving");

		var rooms = GetInt(element, "rooms");
		var storey = GetInt(element, "storey");
		var storeys = GetInt(element, "storeys");

		var address = NormalizeString(GetString(element, "address"));
		var contactName = NormalizeString(GetString(element, "contactName"));
		var contactEmail = NormalizeString(GetString(element, "contactEmail"));
		var contactPhones = GetStringArray(element, "contactPhones");

		var images = GetAllImages(element);

		// цены из priceRates
		decimal? priceUsd = null, priceRub = null;
		if (element.TryGetProperty("priceRates", out var priceRates) && priceRates.ValueKind == JsonValueKind.Object)
		{
			priceUsd = GetDecimalFromObject(priceRates, "840");
			priceRub = GetDecimalFromObject(priceRates, "933");
		}
		else
		{
			// fallback: если price+priceCurrency заданы, подставим соответствующее поле
			var price = GetDecimal(element, "price");
			var currency = GetInt(element, "priceCurrency");
			if (price is not null && currency is not null)
			{
				if (currency == 840) priceUsd = price;
				else if (currency == 933) priceRub = price;
			}
		}

		var createdAt = GetDateTimeOffset(element, "createdAt");
		var updatedAt = GetDateTimeOffset(element, "updatedAt");

		listing = new RealEstateModel
		{
			Title = title,
			Description = description,
			Headline = headline,
			Code = code,
			AreaTotal = areaTotal,
			AreaLiving = areaLiving,
			Images = images,
			ImageUrl = images.FirstOrDefault(), 
			PriceUsd = priceUsd,
			PriceRub = priceRub,
			ContactPhones = contactPhones,
			ContactName = contactName,
			ContactEmail = contactEmail,
			Address = address,
			Rooms = rooms,
			Storey = storey,
			Storeys = storeys,
			CreatedAt = createdAt,
			UpdatedAt = updatedAt
		};

		return !string.IsNullOrEmpty(title)
		       || areaTotal is not null
		       || priceUsd is not null
		       || priceRub is not null
		       || images.Count > 0;
	}

	private static string? GetString(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out var property)) return null;
		return property.ValueKind switch
		{
			JsonValueKind.Null => null,
			JsonValueKind.String => property.GetString(),
			_ => property.ToString()
		};
	}

	private static int? GetInt(JsonElement element, string name)
	{
		if (!element.TryGetProperty(name, out var prop)) return null;
		return prop.ValueKind switch
		{
			JsonValueKind.Number when prop.TryGetInt32(out var i) => i,
			JsonValueKind.String when int.TryParse(prop.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
				out var i) => i,
			_ => null
		};
	}

	private static double? GetDouble(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out var property)) return null;

		return property.ValueKind switch
		{
			JsonValueKind.Number when property.TryGetDouble(out var number) => number,
			JsonValueKind.String when double.TryParse(property.GetString(), NumberStyles.Float,
				CultureInfo.InvariantCulture, out var number) => number,
			_ => null
		};
	}

	private static decimal? GetDecimal(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out var property))
			return null;

		return property.ValueKind switch
		{
			JsonValueKind.Number when property.TryGetDecimal(out var number) => number,
			JsonValueKind.String when decimal.TryParse(
				property.GetString(),
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out var number) => number,
			_ => null
		};
	}

	private static string? GetFirstImage(JsonElement element)
	{
		if (!element.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array) return null;

		foreach (var image in images.EnumerateArray())
		{
			if (image.ValueKind != JsonValueKind.String) 
				continue;
			var value = image.GetString();
			if (!string.IsNullOrWhiteSpace(value)) return value;
		}

		return null;
	}

	private static List<string> GetStringArray(JsonElement element, string propertyName)
	{
		var result = new List<string>();
		if (!element.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
			return result;

		foreach (var it in arr.EnumerateArray())
		{
			if (it.ValueKind != JsonValueKind.String) 
				continue;
			var s = it.GetString();
			if (!string.IsNullOrWhiteSpace(s))
				result.Add(s);
		}

		return result;
	}

	private static List<string> GetAllImages(JsonElement element)
	{
		var result = new List<string>();
		if (!element.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array)
			return result;

		foreach (var image in images.EnumerateArray())
		{
			if (image.ValueKind != JsonValueKind.String) 
				continue;
			var value = image.GetString();
			if (!string.IsNullOrWhiteSpace(value))
				result.Add(value);
		}

		return result;
	}

	private static DateTimeOffset? GetDateTimeOffset(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out var property))
			return null;

		if (property.ValueKind != JsonValueKind.String)
			return null;

		var s = property.GetString();
		if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
			return dto.ToUniversalTime(); 

		return null;
	}

	private static decimal? GetDecimalFromObject(JsonElement obj, string code)
	{
		if (obj.TryGetProperty(code, out var prop))
		{
			return prop.ValueKind switch
			{
				JsonValueKind.Number when prop.TryGetDecimal(out var d) => d,
				JsonValueKind.String when decimal.TryParse(prop.GetString(), NumberStyles.Float,
					CultureInfo.InvariantCulture, out var d) => d,
				_ => null
			};
		}

		return null;
	}

	private static string? NormalizeString(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}
}