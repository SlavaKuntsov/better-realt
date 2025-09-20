using Flatly.Core.Dtos;
using Flatly.Core.RealEstate;

namespace Flatly.Core.Abstractions.Services;

public interface IRealtNextDataParser
{
	IList<RealEstateModel> Parse(string html);
	PaginationInfoDto? ParsePagination(string html);
}