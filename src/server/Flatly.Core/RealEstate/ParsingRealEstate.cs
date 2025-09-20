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
	private const int OtherPagesSize = 30;

	public async Task<IList<RealEstateModel>> Handle(ParsingCommand request, CancellationToken ct)
	{
		var links = options.Value.Links ?? [];
		var all = new List<RealEstateModel>(512);

		foreach (var link in links)
		{
			if (string.IsNullOrWhiteSpace(link))
				continue;

			var baseUri = new Uri(link, UriKind.Absolute);

			// 1) грузим первую страницу (page=1)
			var firstUri = provider.WithPage(baseUri, 1);
			var first = await provider.GetPageAsync(firstUri, ct);
			if (first is null)
				continue;

			// Добавляем объекты первой страницы
			if (first.Items.Count > 0)
				all.AddRange(first.Items);

			// 2) считаем сколько всего страниц
			// Берем фактическое число объектов на первой странице (в JSON это 90 для page=1).
			var totalCount = first.Pagination.TotalCount;
			var firstCount = first.Items.Count;

			if (totalCount <= firstCount)
				continue; // всё уже собрали

			var remaining = totalCount - firstCount;

			// Из условия: все последующие страницы по 30
			var morePages = (int)Math.Ceiling(remaining / (double)OtherPagesSize);
			var totalPages = 1 + morePages;

			// 3) обходим страницы 2..N
			for (var p = 2; p <= totalPages; p++)
			{
				var pageUri = provider.WithPage(baseUri, p);
				var pageRes = await provider.GetPageAsync(pageUri, ct);
				if (pageRes?.Items is { Count: > 0 })
					all.AddRange(pageRes.Items);
			}
		}

		if (all.Count <= 0) return [];
		await dbContext.RealEstates.AddRangeAsync(all, ct);
		await dbContext.SaveChangesAsync(ct);

		return all;
	}
}