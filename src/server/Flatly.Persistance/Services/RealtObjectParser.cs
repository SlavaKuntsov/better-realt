using System.Globalization;
using System.Text.Json;
using Flatly.Core.Abstractions.Services;
using Flatly.Core.RealEstate;
using Microsoft.Extensions.Logging;

namespace Flatly.Persistance.Services;

public sealed class RealtObjectParser(ILogger<RealtObjectParser> logger) : IRealtObjectParser
{
    private const string ScriptMarker = "<script id=\"__NEXT_DATA__\"";
    private const string ScriptEndMarker = "</script>";

    public RealEstateModel? Parse(string html)
    {
        var json = ExtractNextDataJson(html);
        if (string.IsNullOrWhiteSpace(json))
        {
            logger.LogWarning("NEXT_DATA script not found on object page.");
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!TryGetObjectNode(doc.RootElement, out var obj))
                return null;

            var model = new RealEstateModel
            {
                // Идентификаторы / коды
                Code = GetInt(obj, "code"),

                // Заголовки/описание
                Title = Normalize(GetString(obj, "title")),
                Headline = Normalize(GetString(obj, "headline")),
                Description = Normalize(GetString(obj, "description")),

                // Площади
                AreaTotal  = GetDouble(obj, "areaTotal"),
                AreaLiving = GetDouble(obj, "areaLiving"),
                AreaKitchen = GetDouble(obj, "areaKitchen"),

                // Этажи/комнаты/дом
                Rooms = GetInt(obj, "rooms"),
                Storey = GetInt(obj, "storey"),
                Storeys = GetInt(obj, "storeys"),
                BuildingYear = GetInt(obj, "buildingYear"),
                OverhaulYear = GetInt(obj, "overhaulYear"),

                // Параметры
                Layout = Normalize(GetString(obj, "layout")),
                BalconyType = Normalize(GetString(obj, "balconyType")),
                RepairState = Normalize(GetString(obj, "repairState")),
                Furniture = GetBool(obj, "furniture"),
                Toilet = Normalize(GetString(obj, "toilet")),

                // Оплаты/сроки
                Prepayment = Normalize(GetString(obj, "prepayment")),
                HousingRent = Normalize(GetString(obj, "housingRent")),
                LeasePeriod = Normalize(GetString(obj, "leasePeriod")),

                // Контакты
                ContactName = Normalize(GetString(obj, "contactName")),
                ContactEmail = Normalize(GetString(obj, "contactEmail")),
                ContactPhones = GetStringArray(obj, "contactPhones"),

                // Адрес/место
                Address = Normalize(GetString(obj, "address")),
                TownName = Normalize(GetString(obj, "townName")),
                TownDistrictName = Normalize(GetString(obj, "townDistrictName")),
                TownSubDistrictName = Normalize(GetString(obj, "townSubDistrictName")),
                StreetName = Normalize(GetString(obj, "streetName")),
                HouseNumber = GetInt(obj, "houseNumber"),
                BuildingNumber = Normalize(GetString(obj, "buildingNumber")),

                // Прочее
                Seller = Normalize(GetString(obj, "seller")),
                Paid = GetBool(obj, "paid"),
                ViewsCount = GetInt(obj, "viewsCount"),
                CreatedAt = GetDateTimeOffset(obj, "createdAt"),
                UpdatedAt = GetDateTimeOffset(obj, "updatedAt"),
                RaiseDate = GetDateTimeOffset(obj, "raiseDate"),
                NewAgainDate = GetDateTimeOffset(obj, "newAgainDate"),

                // Картинки
                Images = GetAllImages(obj),
            };

            model.ImageUrl = model.Images.FirstOrDefault();

            // Гео (ASSUMPTION: [lon, lat])
            if (obj.TryGetProperty("location", out var loc) && loc.ValueKind == JsonValueKind.Array)
            {
                var idx = 0;
                foreach (var it in loc.EnumerateArray())
                {
                    if (it.ValueKind == JsonValueKind.Number && it.TryGetDouble(out var d))
                    {
                        if (idx == 0) model.Longitude = d;
                        else if (idx == 1) model.Latitude = d;
                    }
                    idx++;
                }
            }

            // Валюты
            if (obj.TryGetProperty("priceRates", out var pr) && pr.ValueKind == JsonValueKind.Object)
            {
                model.PriceUsd    = GetDecimalFromObject(pr, "840");
                model.PriceEur    = GetDecimalFromObject(pr, "978");
                model.PriceRubRus = GetDecimalFromObject(pr, "643");
                model.PriceByn    = GetDecimalFromObject(pr, "933");
                // обратная совместимость с вашим полем PriceRub (изначально с комментом "933")
                model.PriceRub = model.PriceByn ?? model.PriceRub;
            }
            else
            {
                // fallback: price + priceCurrency
                var price = GetDecimal(obj, "price");
                var currency = GetInt(obj, "priceCurrency");
                if (price.HasValue && currency.HasValue)
                {
                    switch (currency.Value)
                    {
                        case 840: model.PriceUsd = price; break;
                        case 978: model.PriceEur = price; break;
                        case 643: model.PriceRubRus = price; break;
                        case 933: model.PriceByn = price; model.PriceRub = price; break;
                    }
                }
            }

            // Appliances
            model.Appliances = GetStringArray(obj, "appliances");

            return model;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse object NEXT_DATA JSON.");
            return null;
        }
    }

    private static string? ExtractNextDataJson(string html)
    {
        var start = html.IndexOf(ScriptMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return null;
        var contentStart = html.IndexOf('>', start);
        if (contentStart < 0) return null;
        contentStart++;
        var end = html.IndexOf(ScriptEndMarker, contentStart, StringComparison.OrdinalIgnoreCase);
        if (end < 0 || end <= contentStart) return null;
        return html.Substring(contentStart, end - contentStart);
    }

    private static bool TryGetObjectNode(JsonElement root, out JsonElement obj)
    {
        obj = default;
        if (!root.TryGetProperty("props", out var props)) return false;
        if (!props.TryGetProperty("pageProps", out var pageProps)) return false;
        if (!pageProps.TryGetProperty("initialState", out var initialState)) return false;
        if (!initialState.TryGetProperty("objectView", out var objectView)) return false;
        if (!objectView.TryGetProperty("object", out var objNode)) return false;
        obj = objNode;
        return obj.ValueKind == JsonValueKind.Object;
    }

    private static string? GetString(JsonElement el, string name)
        => el.TryGetProperty(name, out var p) ? p.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => p.GetString(),
            _ => p.ToString()
        } : null;

    private static int? GetInt(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetInt32(out var i) => i,
            JsonValueKind.String when int.TryParse(p.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
            _ => null
        };
    }

    private static bool? GetBool(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(p.GetString(), out var b) => b,
            _ => null
        };
    }

    private static double? GetDouble(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetDouble(out var d) => d,
            JsonValueKind.String when double.TryParse(p.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            _ => null
        };
    }

    private static decimal? GetDecimal(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(p.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            _ => null
        };
    }

    private static decimal? GetDecimalFromObject(JsonElement obj, string code)
    {
        if (!obj.TryGetProperty(code, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(p.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            _ => null
        };
    }

    private static List<string> GetStringArray(JsonElement el, string name)
    {
        var res = new List<string>();
        if (!el.TryGetProperty(name, out var arr) || arr.ValueKind != JsonValueKind.Array) return res;
        foreach (var it in arr.EnumerateArray())
        {
            if (it.ValueKind == JsonValueKind.String)
            {
                var s = it.GetString();
                if (!string.IsNullOrWhiteSpace(s)) res.Add(s!);
            }
        }
        return res;
    }

    private static List<string> GetAllImages(JsonElement el)
    {
        // приоритет slides (карточка), иначе images (если есть)
        var list = new List<string>();
        if (el.TryGetProperty("slides", out var slides) && slides.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in slides.EnumerateArray())
                if (s.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(s.GetString()))
                    list.Add(s.GetString()!);
        }
        if (list.Count == 0 && el.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in images.EnumerateArray())
                if (s.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(s.GetString()))
                    list.Add(s.GetString()!);
        }
        return list;
    }

    private static string? Normalize(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static DateTimeOffset? GetDateTimeOffset(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.String) return null;
        return DateTimeOffset.TryParse(p.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto)
            ? dto.ToUniversalTime()
            : null;
    }
}
