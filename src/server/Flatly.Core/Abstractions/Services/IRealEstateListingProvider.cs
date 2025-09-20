using Flatly.Core.Dtos;
using Flatly.Core.RealEstate;

namespace Flatly.Core.Abstractions.Services;

public interface IRealEstateListingProvider
{
	Task<ListingsPageDto?> GetPageAsync(Uri link, CancellationToken ct);

	Task<IList<RealEstateModel>> GetListingsAsync(Uri link, CancellationToken ct);

	Uri WithPage(Uri baseLink, int page);
}