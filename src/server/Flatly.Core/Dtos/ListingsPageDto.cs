using Flatly.Core.RealEstate;

namespace Flatly.Core.Dtos;

public sealed record ListingsPageDto(IList<RealEstateModel> Items, PaginationInfoDto Pagination);