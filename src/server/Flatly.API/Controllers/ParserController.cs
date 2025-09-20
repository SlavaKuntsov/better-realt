using Asp.Versioning;
using Common.Contracts;
using Flatly.Core.RealEstate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flatly.API.Controllers;

/// <inheritdoc />
[ApiController]
[Route("api/v{version:apiVersion}/parser")]
[ApiVersion("1.0")]
public class ParserController(IMediator mediator) : ControllerBase
{
	/// <summary>
	/// Возвращает и сохраняет в бд список квартир с realt.by
	/// </summary>
	/// <param name="ct"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<IActionResult> InitialParsing(CancellationToken ct = default)
	{
		var listings = await mediator.Send(new ParsingCommand(), ct);
		return Ok(new ApiResponse<IList<RealEstateModel>>(StatusCodes.Status200OK, listings, listings.Count));
	}
}