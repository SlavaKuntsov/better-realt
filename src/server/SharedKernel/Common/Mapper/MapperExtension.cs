using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Mapper;

public static class MapperExtension
{
	public static IServiceCollection AddMapper(this IServiceCollection services)
	{
		var config = TypeAdapterConfig.GlobalSettings;
		config.Scan(AppDomain.CurrentDomain.GetAssemblies());
		services.AddSingleton(config);
		services.AddSingleton<IMapper>(new MapsterMapper.Mapper(config));
		return services;
	}
}