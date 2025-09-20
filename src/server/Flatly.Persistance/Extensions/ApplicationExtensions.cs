using Flatly.Persistance.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flatly.Persistance.Extensions;

public static class ApplicationExtensions
{
	public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
	{
		using var scope = app.ApplicationServices.CreateScope();
		var services = scope.ServiceProvider;
		var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

		try
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.Migrate();
			logger.LogInformation("Database migrations applied successfully");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occurred while applying database migrations");

			throw;
		}

		return app;
	}
}