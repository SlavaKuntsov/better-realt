using Flatly.Core.Abstractions.Services;
using Flatly.Core.RealEstate;
using Microsoft.Extensions.Logging;

namespace Flatly.Persistance.Services;

public sealed class RealEstateListingProvider(
	HttpClient httpClient,
	IRealtNextDataParser parser,
	ILogger<RealEstateListingProvider> logger)
	: IRealEstateListingProvider
{
	public async Task<IList<RealEstateModel>> GetListingsAsync(Uri link, CancellationToken ct)
	{
		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, link);
			using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

			response.EnsureSuccessStatusCode();

			var html = await response.Content.ReadAsStringAsync(ct);
			return parser.Parse(html);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "Failed to download listings page from {Uri}.", link);
			return [];
		}
	}
}