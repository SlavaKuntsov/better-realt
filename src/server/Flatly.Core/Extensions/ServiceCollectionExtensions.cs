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
		
		return services;
	}
}