using Flatly.Core.Abstractions.Data;
using Flatly.Core.Abstractions.Services;
using Flatly.Core.Dtos;
using MediatR;
using Microsoft.Extensions.Options;

namespace Flatly.Core.RealEstate;

public sealed record ParsingCommand : IRequest<IList<RealEstateModel>>;

public sealed class ParsingCommandHandler(
	IApplicationDbContext dbContext,
	IRealEstateListingProvider provider,
	IOptions<ParsingOptions> options)
	: IRequestHandler<ParsingCommand, IList<RealEstateModel>>
{
	public async Task<IList<RealEstateModel>> Handle(ParsingCommand request, CancellationToken ct)
	{
		var links = options.Value.Links;
		var all = new List<RealEstateModel>(256);

		foreach (var link in links)
		{
			var uri = new Uri(link, UriKind.Absolute);
			var page = await provider.GetListingsAsync(uri, ct);
			if (page.Count > 0)
				all.AddRange(page);
		}

		if (all.Count <= 0) return [];
		await dbContext.RealEstates.AddRangeAsync(all, ct);
		await dbContext.SaveChangesAsync(ct);

		return all;
	}
}