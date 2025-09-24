using Asp.Versioning;
using Common.Contracts;
using Flatly.Core.RealEstate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flatly.API.Controllers;

/// <inheritdoc />
[ApiController]
[Route("api/v{version:apiVersion}/real-estate")]
[ApiVersion("1.0")]
public class RealEstateController(IMediator mediator) : ControllerBase
{
	/// <summary>
	///     Возвращает список объектов недвижимости.
	/// </summary>
	/// <param name="code">Код объекта. Фильтр по точному коду.</param>
	/// <param name="minPriceUsd">Минимальная цена USD.</param>
	/// <param name="maxPriceUsd">Максимальная цена USD.</param>
	/// <param name="sortBy">Сортировка: code, title, priceUsd, area.</param>
	/// <param name="descending">Признак обратной сортировки.</param>
	/// <param name="limit">Сколько записей вернуть.</param>
	/// <param name="offset">Смещение.</param>
	/// <param name="ct"></param>
	/// <returns>Список недвижимости.</returns>
	[HttpGet]
	public async Task<IActionResult> Get([FromQuery] int? code,
		[FromQuery] decimal? minPriceUsd,
		[FromQuery] decimal? maxPriceUsd,
		[FromQuery] string? sortBy,
		[FromQuery] bool descending = false,
		[FromQuery] byte limit = 10,
		[FromQuery] byte offset = 1,
		CancellationToken ct = default)
	{
		var response = await mediator.Send(new GetRealEstatesQuery(
			limit,
			offset,
			code,
			minPriceUsd,
			maxPriceUsd,
			sortBy,
			descending), ct);
		return Ok(response);
	}

	/// <summary>
	///     Возвращает и сохраняет в бд список квартир с realt.by
	/// </summary>
	/// <param name="ct"></param>
	/// <returns>Список кодов всей недвижимости.</returns>
	[HttpGet("initial-parsing")]
	public async Task<IActionResult> InitialParsing(CancellationToken ct = default)
	{
		var listings = await mediator.Send(new ParsingCommand(), ct);
		return Ok(new ApiResponse<IList<int?>>(listings));
	}
}