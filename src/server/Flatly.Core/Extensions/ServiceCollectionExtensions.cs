using Flatly.Core.Options;
using Flatly.Core.RealEstate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flatly.Core.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ParsingOptions>(configuration.GetSection(nameof(ParsingOptions)));
		
		services.AddMediatR(
			cfg =>
			{
				cfg.RegisterServicesFromAssemblyContaining<ParsingCommandHandler>();
			});

		var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			Console.Error.WriteLine("GROQ_API_KEY не задан. export/set переменную окружения и перезапусти процесс.");
			throw new Exception("GROQ_API_KEY не задан. export/set переменную окружения и перезапусти процесс.");
		}

		Console.WriteLine(apiKey);
		
		return services;
	}
}