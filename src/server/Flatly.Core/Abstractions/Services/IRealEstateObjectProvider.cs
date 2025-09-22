namespace Flatly.Core.Abstractions.Services;

public interface IRealEstateObjectProvider
{
	Task<string?> GetObjectHtmlAsync(int code, CancellationToken ct);
}