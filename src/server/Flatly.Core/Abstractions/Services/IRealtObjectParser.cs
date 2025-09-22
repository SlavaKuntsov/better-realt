using Flatly.Core.RealEstate;

namespace Flatly.Core.Abstractions.Services;

public interface IRealtObjectParser
{
	RealEstateModel? Parse(string html);
}