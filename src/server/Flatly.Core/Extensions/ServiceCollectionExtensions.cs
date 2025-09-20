using Flatly.Core.RealEstate;
using Microsoft.Extensions.DependencyInjection;

namespace Flatly.Core.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services)
	{
		services.AddMediatR(
			cfg =>
			{
				cfg.RegisterServicesFromAssemblyContaining<ParsingCommandHandler>();
			});
		
		return services;
	}
}