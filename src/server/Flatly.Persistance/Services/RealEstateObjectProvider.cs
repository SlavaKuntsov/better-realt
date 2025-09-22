using Flatly.Core.Abstractions.Services;
using Flatly.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flatly.Persistance.Services;

public sealed class RealEstateObjectProvider(
	HttpClient httpClient,
	IOptions<ParsingOptions> options,
	ILogger<RealEstateObjectProvider> logger) : IRealEstateObjectProvider
{
	public async Task<string?> GetObjectHtmlAsync(int code, CancellationToken ct)
	{
		var url = BuildUrl(code);
		try
		{
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			using var resp = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
			resp.EnsureSuccessStatusCode();
			return await resp.Content.ReadAsStringAsync(ct);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (HttpRequestException ex)
		{
			logger.LogWarning(ex, "Failed to download object page {Url}.", url);
			return null;
		}
	}

	private string BuildUrl(int code)
	{
		var template = options.Value.ObjectUrlTemplate ?? "https://realt.by/rent-flat-for-long/object/{code}/";
		return template.Replace("{code}", code.ToString());
	}
}