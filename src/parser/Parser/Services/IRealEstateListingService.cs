using Parser.Models;

namespace Parser.Services;

public interface IRealEstateListingService
{
    Task<IReadOnlyList<RealEstateListing>> GetListingsAsync(CancellationToken cancellationToken);
}
