using System.Security.Claims;
using Common.Common;
using Common.Exceptions;
using Common.Host;
using Common.OpenApi;
using DotNetEnv;
using Flatly.Core.Extensions;
using Flatly.Persistance.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Env.Load("./../../.env");

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) =>
	config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());

services
	.AddCommon()
	.AddExceptions()
	// .AddAuthorization(configuration)
	// .AddMapper()
	// .AddSwagger()
	.AddSwaggerWithAuth(configuration);

services.AddAuthorization();
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(o =>
	{
		o.RequireHttpsMetadata = false;
		o.Audience = configuration["Authorization:Audience"];
		o.MetadataAddress = configuration["Authorization:MetadataAddress"]!;
		o.TokenValidationParameters = new TokenValidationParameters
		{
			ValidIssuer = configuration["Authorization:ValidateIssuer"]
		};
	});

services
	.AddCore(configuration)
	.AddPersistence(configuration);

var app = builder.Build();

app.ApplyMigrations();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1"); });

// app.MapGet("users/me", (ClaimsPrincipal claimsPrincipal) =>
// {
// 	return claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
// }).RequireAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.AddPortsLogging();

app.Run();