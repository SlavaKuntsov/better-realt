using Flatly.Core.RealEstate;

namespace Flatly.Core.Abstractions.Services;

public interface IRealEstateListingProvider
{
	Task<IList<RealEstateModel>> GetListingsAsync(Uri link, CancellationToken ct);
}