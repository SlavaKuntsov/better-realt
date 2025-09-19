using System.Globalization;
using System.Text.Json;
using Parser.Models;

namespace Parser.Services;

public sealed class RealEstateListingService(HttpClient httpClient, ILogger<RealEstateListingService> logger)
    : IRealEstateListingService
{
    private const string ScriptMarker = "<script id=\"__NEXT_DATA__\"";
    private const string ScriptEndMarker = "</script>";
    private static readonly Uri ListingsUri = new("https://realt.by/sale/flats/2k/");

    public async Task<IReadOnlyList<RealEstateListing>> GetListingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ListingsUri);
            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
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

                var listings = new List<RealEstateListing>();
                foreach (var item in objectsElement.EnumerateArray())
                    if (TryParseListing(item, out var listing))
                        listings.Add(listing);

                return listings;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse JSON payload from __NEXT_DATA__ script.");
                return [];
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to download listings page from {Uri}.", ListingsUri);
            return [];
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

    private static bool TryParseListing(JsonElement element, out RealEstateListing listing)
    {
        var title = NormalizeString(GetString(element, "title")) ?? string.Empty;
        var description = NormalizeString(GetString(element, "description"));
        var areaTotal = GetDouble(element, "areaTotal");
        var imageUrl = NormalizeString(GetFirstImage(element));

        listing = new RealEstateListing(title, description, areaTotal, imageUrl);
        return !string.IsNullOrEmpty(title) || description is not null || areaTotal is not null || imageUrl is not null;
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

    private static string? GetFirstImage(JsonElement element)
    {
        if (!element.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array) return null;

        foreach (var image in images.EnumerateArray())
            if (image.ValueKind == JsonValueKind.String)
            {
                var value = image.GetString();
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

        return null;
    }

    private static string? NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}