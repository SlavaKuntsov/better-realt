using Parser.Models;
using Parser.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IRealEstateListingService, RealEstateListingService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");
    client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.MapGet("/api/listings", async (IRealEstateListingService service, CancellationToken cancellationToken) =>
    {
        var listings = await service.GetListingsAsync(cancellationToken);
        return Results.Ok(listings);
    })
    .WithName("GetRealEstateListings")
    .WithDescription("Возвращает список двухкомнатных квартир с realt.by.")
    .Produces<IReadOnlyList<RealEstateListing>>()
    .WithOpenApi();

app.Run();