using Common.Common;
using Common.Exceptions;
using Common.Host;
using Common.OpenApi;
using DotNetEnv;
using Flatly.Core.Dtos;
using Flatly.Core.Extensions;
using Flatly.Persistance.Extensions;
using Serilog;

Env.Load("./../../.env");

var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
	Console.Error.WriteLine("GROQ_API_KEY не задан. export/set переменную окружения и перезапусти процесс.");
	throw new Exception("GROQ_API_KEY не задан. export/set переменную окружения и перезапусти процесс.");
}

Console.WriteLine(apiKey);

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) =>
	config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());

services.Configure<ParsingOptions>(configuration.GetSection(nameof(ParsingOptions)));

services
	.AddCommon()
	.AddExceptions()
	// .AddAuthorization(configuration)
	// .AddMapper()
	.AddSwagger()
	.AddOpenApi();

services
	.AddCore()
	.AddPersistence(configuration);

var app = builder.Build();

app.ApplyMigrations();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1"); });

if (app.Environment.IsDevelopment())
	app.MapOpenApi();

// app.UseCookiePolicy(
// 	new CookiePolicyOptions
// 	{
// 		MinimumSameSitePolicy = SameSiteMode.None,
// 		HttpOnly = HttpOnlyPolicy.Always,
// 		Secure = CookieSecurePolicy.Always
// 	});

app.UseHttpsRedirection();

// app.UseForwardedHeaders(
// 	new ForwardedHeadersOptions
// 	{
// 		ForwardedHeaders = ForwardedHeaders.All
// 	});
// app.UseCors();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.AddPortsLogging();

app.Run();