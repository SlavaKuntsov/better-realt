using Common.Contracts;
using Flatly.Core.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flatly.Core.RealEstate;

using Response = ApiResponse<IList<RealEstateModel>>;

public sealed record GetRealEstatesQuery(
	byte Limit,
	byte Offset,
	int? Code = null,
	decimal? MinPriceUsd = null,
	decimal? MaxPriceUsd = null,
	string? SortBy = null,
	bool Descending = false) : IRequest<Response>;

public sealed class GetRealEstatesQueryHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetRealEstatesQuery, Response>
{
	public async Task<Response> Handle(GetRealEstatesQuery request, CancellationToken ct)
	{
		var query = dbContext.RealEstates.AsQueryable();

		if (request.Code.HasValue)
			query = query.Where(p => p.Code == request.Code.Value);

		if (request.MinPriceUsd.HasValue)
			query = query.Where(p => p.PriceUsd >= request.MinPriceUsd.Value);

		if (request.MaxPriceUsd.HasValue)
			query = query.Where(p => p.PriceUsd <= request.MaxPriceUsd.Value);

		if (!string.IsNullOrWhiteSpace(request.SortBy))
		{
			query = request.SortBy.ToLower() switch
			{
				"code" => request.Descending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
				"title" => request.Descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
				"priceUsd" => request.Descending
					? query.OrderByDescending(p => p.PriceUsd)
					: query.OrderBy(p => p.PriceUsd),
				"area" => request.Descending
					? query.OrderByDescending(p => p.AreaTotal)
					: query.OrderBy(p => p.AreaTotal),
				_ => query
			};
		}
		else
			query = query.OrderBy(p => p.Code);

		var total = await query.CountAsync(ct);

		query = query.Skip((request.Offset - 1) * request.Limit)
			.Take(request.Limit);

		var items = await query.ToListAsync(ct);

		return new Response(items, request.Limit, request.Offset, null, total);
	}
}