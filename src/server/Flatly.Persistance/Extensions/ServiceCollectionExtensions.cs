using Databases;
using Flatly.Core.Abstractions.Data;
using Flatly.Core.Abstractions.Services;
using Flatly.Persistance.DataAccess;
using Flatly.Persistance.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flatly.Persistance.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddPostgres<IApplicationDbContext, ApplicationDbContext>(configuration);
		
		services.AddScoped<IRealtNextDataParser, RealtNextDataParser>();

		services.AddHttpClient<IRealEstateListingProvider, RealEstateListingProvider>(client =>
		{
			client.DefaultRequestHeaders.UserAgent.ParseAdd(
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");
			client.DefaultRequestHeaders.Accept.ParseAdd(
				"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
		});

		return services;
	}
}