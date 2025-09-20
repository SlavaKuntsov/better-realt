using Asp.Versioning;
using Flatly.Core.RealEstate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Flatly.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/parser")]
[ApiVersion("1.0")]
public class ParserController(IMediator mediator) : ControllerBase
{
	/// <summary>
	/// Возвращает список двухкомнатных квартир с realt.by
	/// </summary>
	/// <param name="ct"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<IActionResult> Get(CancellationToken ct = default)
	{
		var listings = await mediator.Send(new ParsingCommand(), ct);
		return Ok(listings);
	}
}