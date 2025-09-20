using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Common.Host;

public static class HostExtension
{
	public static WebApplicationBuilder UseHttps(this WebApplicationBuilder builder)
	{
		var environment = builder.Environment;

		var portString = Environment.GetEnvironmentVariable("PORT");

		if (string.IsNullOrEmpty(portString))
			portString = builder.Configuration.GetValue<string>("ApplicationSettings:Port");

		if (!int.TryParse(portString, out var port))
			throw new InvalidOperationException($"Invalid port value: {portString}");

		if (environment.IsProduction())
		{
			const string certPath = "/app/localhost.pfx";
			const string certPassword = "1";

			builder.WebHost.ConfigureKestrel(
				options =>
				{
					options.ListenAnyIP(
						port,
						listenOptions =>
						{
							listenOptions.UseHttps(certPath, certPassword);
						});
				});
		}
		else
		{
			builder.WebHost.ConfigureKestrel(
				options =>
				{
					options.ListenAnyIP(
						port,
						listenOptions =>
						{
							listenOptions.UseHttps();
						});
				});
		}

		return builder;
	}

	public static IApplicationBuilder AddPortsLogging(this IApplicationBuilder app)
	{
		app.ApplicationServices
			.GetRequiredService<IHostApplicationLifetime>()
			.ApplicationStarted.Register(() =>
			{
				var server = app.ApplicationServices.GetRequiredService<IServer>();
				var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
				if (addresses == null) return;
				foreach (var address in addresses)
				{
					// Serilog
					Log.Information("Application is listening on: {Address}/swagger", address);
				}
			});

		return app;
	}
}