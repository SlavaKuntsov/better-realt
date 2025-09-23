using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flatly.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthControllers : ControllerBase
{
	/// <summary>
	///     Возвращает все Claims текущего пользователя
	/// </summary>
	[HttpGet("me")]
	[Authorize]
	public IActionResult GetCurrentUserClaims()
	{
		var claims = HttpContext.User.Claims
			.ToDictionary(c => c.Type, c => c.Value);

		return Ok(claims);
	}
}