using System.Collections.Concurrent;
using System.Threading.Channels;
using Flatly.Core.Abstractions.Data;
using Flatly.Core.Abstractions.Services;
using Flatly.Core.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flatly.Core.RealEstate;

public sealed record ParsingCommand : IRequest<IList<int?>>;

public sealed class ParsingCommandHandler(
	IApplicationDbContext dbContext,
	IRealEstateListingProvider listingsProvider,
	IRealEstateObjectProvider objectProvider,
	IRealtObjectParser objectParser,
	IOptions<ParsingOptions> opt,
	ILogger<ParsingCommandHandler> logger)
	: IRequestHandler<ParsingCommand, IList<int?>>
{
	private const int OtherPagesSize = 30;

	public async Task<IList<int?>> Handle(ParsingCommand request, CancellationToken ct)
	{
		var options = opt.Value;
		var links = options.Links.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
		if (links.Length == 0) return [];

		// 1) Собираем все коды из списков
		var codesSet = await CollectAllCodesAsync(links, options.PageConcurrency, ct);
		var codes = codesSet.ToArray(); // важно: массив нужен для SQL-перевода Contains
		logger.LogInformation("Collected {Count} unique codes from listings.", codes.Length);
		if (codes.Length == 0) return [];

		// 2) Канал для карточек
		var capacity = Math.Max(2 * options.DetailConcurrency, 64);
		var channel = Channel.CreateBounded<RealEstateModel>(new BoundedChannelOptions(capacity)
		{
			SingleWriter = false, SingleReader = true, FullMode = BoundedChannelFullMode.Wait
		});

		var produced = 0;
		var succeeded = 0;
		var failed = 0;

		// Метрики апдейта/вставки (собирает consumer)
		var insertedCodes = new List<int>(capacity);
		var updatedCodes = new List<int>(capacity);

		// 3) Потребитель: пакетный upsert
		var consumer = Task.Run(async () =>
		{
			var batch = new List<RealEstateModel>(options.SaveBatchSize);
			await foreach (var item in channel.Reader.ReadAllAsync(ct))
			{
				batch.Add(item);

				if (batch.Count >= options.SaveBatchSize)
				{
					await UpsertBatchAsync(batch, options.SkipUnchanged, insertedCodes, updatedCodes, ct);
					batch.Clear();
				}
			}

			if (batch.Count > 0)
				await UpsertBatchAsync(batch, options.SkipUnchanged, insertedCodes, updatedCodes, ct);
		}, ct);

		// 4) Производители: качаем карточки параллельно
		var rnd = options is { ThrottleMinMs: > 0, ThrottleMaxMs: > 0 } ? new Random() : null;
		var throttleMin = options.ThrottleMinMs ?? 0;
		var throttleMax = options.ThrottleMaxMs ?? 0;

		await Parallel.ForEachAsync(
			codes,
			new ParallelOptions { MaxDegreeOfParallelism = options.DetailConcurrency, CancellationToken = ct },
			async (code, token) =>
			{
				Interlocked.Increment(ref produced);
				try
				{
					if (rnd != null)
						await Task.Delay(rnd.Next(throttleMin, throttleMax + 1), token);

					var html = await objectProvider.GetObjectHtmlAsync(code, token);
					if (html is null)
					{
						Interlocked.Increment(ref failed);
						return;
					}

					var model = objectParser.Parse(html);
					if (model?.Code is null)
					{
						Interlocked.Increment(ref failed);
						return;
					}

					await channel.Writer.WriteAsync(model, token);
					Interlocked.Increment(ref succeeded);
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Failed to process code={Code}.", code);
					Interlocked.Increment(ref failed);
				}
			});

		channel.Writer.Complete();
		await consumer;

		// 5) Удаляем записи, которых нет в текущем парсинге
		// (ВАЖНО: сначала берём список кодов на удаление для вывода, затем выполняем bulk delete)
		var toDeleteCodes = await dbContext.RealEstates
			.Where(e => e.Code != null && !codes.Contains(e.Code.Value))
			.Select(e => e.Code!.Value)
			.ToListAsync(ct);

		var deletedCount = 0;
		if (toDeleteCodes.Count > 0)
		{
			deletedCount = await dbContext.RealEstates
				.Where(e => e.Code != null && toDeleteCodes.Contains(e.Code!.Value))
				.ExecuteDeleteAsync(ct);
		}

		logger.LogInformation("Objects processed: total={Total}, ok={Ok}, failed={Failed}.",
			produced, succeeded, failed);

		// 6) Текущие числа и вывод списков
		var totalParsed = codes.Length;

		// На всякий случай считаем по Distinct (дубликатов быть не должно, но так надёжнее)
		var insertedDistinct = insertedCodes.Distinct().OrderBy(x => x).ToList();
		var updatedDistinct = updatedCodes.Distinct().OrderBy(x => x).ToList();

		var newCount = insertedDistinct.Count;
		var updatedCount = updatedDistinct.Count;

		var currentDbTotal = await dbContext.RealEstates.CountAsync(ct);

		// Логи
		logger.LogInformation(
			"SUMMARY: parsed={Parsed}, inserted={Inserted}, updated={Updated}, deleted={Deleted}, db_total_now={DbTotal}",
			totalParsed, newCount, updatedCount, deletedCount, currentDbTotal);

		if (newCount > 0)
			logger.LogInformation("INSERTED codes: {Codes}", string.Join(", ", insertedDistinct));

		if (updatedCount > 0)
			logger.LogInformation("UPDATED codes: {Codes}", string.Join(", ", updatedDistinct));

		if (deletedCount > 0)
		{
			logger.LogInformation("DELETED codes: {Codes}",
				string.Join(", ", toDeleteCodes.Distinct().OrderBy(x => x)));
		}

		// Вернём все актуальные коды из БД (как у вас было)
		var ids = await dbContext.RealEstates
			.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
			.AsNoTracking()
			.Select(re => re.Code)
			.ToListAsync(ct);

		return ids;
	}

	private async Task<HashSet<int>> CollectAllCodesAsync(IEnumerable<string> links, int pageConcurrency,
		CancellationToken ct)
	{
		var codes = new ConcurrentDictionary<int, byte>();

		foreach (var link in links)
		{
			var baseUri = new Uri(link, UriKind.Absolute);

			// page=1
			var firstUri = listingsProvider.WithPage(baseUri, 1);
			var first = await listingsProvider.GetPageAsync(firstUri, ct);
			if (first is null) continue;

			AddCodesFromListings(first.Items);

			var totalCount = first.Pagination.TotalCount;
			var firstCount = first.Items.Count;
			if (totalCount <= firstCount) continue;

			var remaining = totalCount - firstCount;
			var morePages = (int)Math.Ceiling(remaining / (double)OtherPagesSize);
			var totalPages = 1 + morePages;

			var pages = Enumerable.Range(2, totalPages - 1).ToArray();

			await Parallel.ForEachAsync(
				pages,
				new ParallelOptions { MaxDegreeOfParallelism = pageConcurrency, CancellationToken = ct },
				async (p, token) =>
				{
					var pageUri = listingsProvider.WithPage(baseUri, p);
					var res = await listingsProvider.GetPageAsync(pageUri, token);
					if (res?.Items is { Count: > 0 })
						AddCodesFromListings(res.Items);
				});
		}

		return codes.Keys.ToHashSet();

		void AddCodesFromListings(IList<RealEstateModel> items)
		{
			foreach (var it in items)
			{
				if (it.Code is int c)
					codes.TryAdd(c, 0);
			}
		}
	}

	private async Task UpsertBatchAsync(
		List<RealEstateModel> batch,
		bool skipUnchanged,
		List<int> insertedCodes,
		List<int> updatedCodes,
		CancellationToken ct)
	{
		var incoming = batch.Where(x => x.Code.HasValue).ToList();
		if (incoming.Count == 0) return;

		var codes = incoming.Select(x => x.Code!.Value).Distinct().ToArray();

		var existing = await dbContext.RealEstates
			.Where(e => e.Code != null && codes.Contains(e.Code.Value))
			.ToDictionaryAsync(e => e.Code!.Value, ct);

		// управляем ChangeTracker через реальный DbContext
		var prevDetect = dbContext.ChangeTracker.AutoDetectChangesEnabled;
		dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

		try
		{
			var toInsert = new List<RealEstateModel>(incoming.Count);

			foreach (var src in incoming)
			{
				if (existing.TryGetValue(src.Code!.Value, out var dbEntity))
				{
					// Пропускаем, если не изменилось (по UpdatedAt) — опционально
					if (skipUnchanged && src.UpdatedAt.HasValue && dbEntity.UpdatedAt.HasValue &&
					    src.UpdatedAt.Value == dbEntity.UpdatedAt.Value)
						continue;

					CopyFields(dbEntity, src);
					updatedCodes.Add(src.Code!.Value);
				}
				else
				{
					toInsert.Add(src);
					insertedCodes.Add(src.Code!.Value);
				}
			}

			if (toInsert.Count > 0)
				await dbContext.RealEstates.AddRangeAsync(toInsert, ct);

			await dbContext.SaveChangesAsync(ct);
		}
		finally
		{
			dbContext.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
		}
	}

	private static void CopyFields(RealEstateModel dst, RealEstateModel src)
	{
		// переносим все поля, кроме Id/Code
		dst.Title = src.Title;
		dst.Description = src.Description;
		dst.Headline = src.Headline;

		dst.AreaTotal = src.AreaTotal;
		dst.AreaLiving = src.AreaLiving;
		dst.AreaKitchen = src.AreaKitchen;

		dst.Images = src.Images.ToList();
		dst.ImageUrl = src.ImageUrl;

		dst.PriceUsd = src.PriceUsd;
		dst.PriceRub = src.PriceRub;
		dst.PriceByn = src.PriceByn;
		dst.PriceRubRus = src.PriceRubRus;
		dst.PriceEur = src.PriceEur;

		dst.ContactPhones = src.ContactPhones.ToList();
		dst.ContactName = src.ContactName;
		dst.ContactEmail = src.ContactEmail;

		dst.Address = src.Address;
		dst.Rooms = src.Rooms;
		dst.Storey = src.Storey;
		dst.Storeys = src.Storeys;

		dst.BuildingYear = src.BuildingYear;
		dst.OverhaulYear = src.OverhaulYear;

		dst.Layout = src.Layout;
		dst.BalconyType = src.BalconyType;
		dst.RepairState = src.RepairState;
		dst.Furniture = src.Furniture;
		dst.Toilet = src.Toilet;

		dst.Prepayment = src.Prepayment;
		dst.HousingRent = src.HousingRent;
		dst.LeasePeriod = src.LeasePeriod;
		dst.Appliances = src.Appliances.ToList();

		dst.Longitude = src.Longitude;
		dst.Latitude = src.Latitude;

		dst.TownName = src.TownName;
		dst.TownDistrictName = src.TownDistrictName;
		dst.TownSubDistrictName = src.TownSubDistrictName;
		dst.StreetName = src.StreetName;
		dst.HouseNumber = src.HouseNumber;
		dst.BuildingNumber = src.BuildingNumber;

		dst.Seller = src.Seller;
		dst.Paid = src.Paid;
		dst.ViewsCount = src.ViewsCount;

		dst.RaiseDate = src.RaiseDate;
		dst.NewAgainDate = src.NewAgainDate;

		dst.CreatedAt = src.CreatedAt;
		dst.UpdatedAt = src.UpdatedAt;
	}
}