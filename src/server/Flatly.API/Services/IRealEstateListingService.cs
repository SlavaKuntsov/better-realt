using Flatly.API.Models;

namespace Flatly.API.Services;

public interface IRealEstateListingService
{
    Task<IReadOnlyList<RealEstateListing>> GetListingsAsync(CancellationToken cancellationToken);
}
