using System.Text.RegularExpressions;
using Flatly.Core.Abstractions.Services;
using Flatly.Core.Dtos;
using Flatly.Core.RealEstate;
using Microsoft.Extensions.Logging;

namespace Flatly.Persistance.Services;

public sealed class RealEstateListingProvider(
	HttpClient httpClient,
	IRealtNextDataParser parser,
	ILogger<RealEstateListingProvider> logger)
	: IRealEstateListingProvider
{
	public async Task<ListingsPageDto?> GetPageAsync(Uri link, CancellationToken ct)
	{
		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, link);
			using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
			response.EnsureSuccessStatusCode();

			var html = await response.Content.ReadAsStringAsync(ct);

			var items = parser.Parse(html);
			var p = parser.ParsePagination(html) ?? new PaginationInfoDto(1, items.Count, items.Count);

			return new ListingsPageDto(items, p);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "Failed to download listings page from {Uri}.", link);
			return null;
		}
	}

	public async Task<IList<RealEstateModel>> GetListingsAsync(Uri link, CancellationToken ct)
	{
		var page = await GetPageAsync(link, ct);
		return page?.Items ?? [];
	}

	public Uri WithPage(Uri baseLink, int page)
	{
		var url = baseLink.ToString();
		if (url.Contains("page=", StringComparison.OrdinalIgnoreCase))
		{
			url = Regex.Replace(url, @"([?&]page=)\d+", $"$1{page}");
			return new Uri(url, UriKind.Absolute);
		}

		var sep = url.Contains('?') ? '&' : '?';
		return new Uri($"{url}{sep}page={page}", UriKind.Absolute);
	}
}