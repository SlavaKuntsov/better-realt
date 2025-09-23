using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Common.OpenApi;

public static class SwaggerExtension
{
	public static IServiceCollection AddSwagger(this IServiceCollection services)
	{
		var apiVersioningBuilder = services.AddApiVersioning(
			o =>
			{
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.DefaultApiVersion = new ApiVersion(1, 0);
				o.ReportApiVersions = true;
				o.ApiVersionReader = new UrlSegmentApiVersionReader();
			});

		apiVersioningBuilder.AddApiExplorer(
			options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});

		services.AddSwaggerGen(
			options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API v1", Version = "v1" });

				var basePath = AppContext.BaseDirectory;
				var apiXml = Path.Combine(basePath, "Flatly.API.xml");
				if (File.Exists(apiXml))
					options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);

				// options.ExampleFilters();

				options.AddSecurityDefinition(
					"Bearer",
					new OpenApiSecurityScheme
					{
						Name = "Authorization",
						In = ParameterLocation.Header,
						Type = SecuritySchemeType.Http,
						Scheme = "Bearer",
						BearerFormat = "JWT",
						Description = "Введите токен JWT в формате 'Bearer {токен}'"
					});

				options.AddSecurityRequirement(
					new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "Bearer"
								}
							},
							[]
						}
					});
			});

		return services;
	}

	public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services, IConfiguration configuration)
	{
		var apiVersioningBuilder = services.AddApiVersioning(
			o =>
			{
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.DefaultApiVersion = new ApiVersion(1, 0);
				o.ReportApiVersions = true;
				o.ApiVersionReader = new UrlSegmentApiVersionReader();
			});

		apiVersioningBuilder.AddApiExplorer(
			options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});
		
		services.AddSwaggerGen(
			options =>
			{
				options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
				
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API v1", Version = "v1" });

				var basePath = AppContext.BaseDirectory;
				var apiXml = Path.Combine(basePath, "Flatly.API.xml");
				if (File.Exists(apiXml))
					options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);

				// options.ExampleFilters();

				options.AddSecurityDefinition(
					"Keycloak",
					new OpenApiSecurityScheme
					{
						Type = SecuritySchemeType.OAuth2,
						Flows = new OpenApiOAuthFlows
						{
							Implicit = new OpenApiOAuthFlow
							{
								AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
								Scopes = new Dictionary<string, string>
								{
									{ "openid", "openid"},
									{ "profile", "profile"}
								}
							}
						}
					});

				options.AddSecurityRequirement(
					new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "Keycloak"
								},
								In = ParameterLocation.Header,
								Name = "Bearer",
								Scheme = "Bearer"
							},
							[]
						}
					});
			});

		return services;
	}
}