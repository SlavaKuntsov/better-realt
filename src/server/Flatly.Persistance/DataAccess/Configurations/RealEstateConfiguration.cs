using Flatly.Core.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Flatly.Persistance.DataAccess.Configurations;

public class RealEstateConfiguration : IEntityTypeConfiguration<RealEstateModel>
{
	public void Configure(EntityTypeBuilder<RealEstateModel> b)
	{
		b.ToTable("real_estates");

		b.HasKey(x => x.Id);
		b.Property(x => x.Id).ValueGeneratedOnAdd();

		// Все DateTimeOffset? → хранить в UTC
		var dtoUtcConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
			v => v.HasValue ? v.Value.ToUniversalTime() : v,
			v => v // читаем как есть
		);

		// ===== Тексты/строки =====
		b.Property(x => x.Title).HasMaxLength(256);
		b.Property(x => x.Description).HasColumnType("text");
		b.Property(x => x.Headline).HasMaxLength(2000);

		b.Property(x => x.Address).HasMaxLength(512);
		b.Property(x => x.ImageUrl).HasMaxLength(1024);

		b.Property(x => x.ContactName).HasMaxLength(256);
		b.Property(x => x.ContactEmail).HasMaxLength(256);

		b.Property(x => x.Layout).HasMaxLength(128);
		b.Property(x => x.BalconyType).HasMaxLength(128);
		b.Property(x => x.RepairState).HasMaxLength(128);
		b.Property(x => x.Toilet).HasMaxLength(128);

		b.Property(x => x.Prepayment).HasMaxLength(64);
		b.Property(x => x.HousingRent).HasMaxLength(64);
		b.Property(x => x.LeasePeriod).HasMaxLength(64);

		b.Property(x => x.TownName).HasMaxLength(256);
		b.Property(x => x.TownDistrictName).HasMaxLength(256);
		b.Property(x => x.TownSubDistrictName).HasMaxLength(256);
		b.Property(x => x.StreetName).HasMaxLength(256);
		b.Property(x => x.BuildingNumber).HasMaxLength(64);
		b.Property(x => x.Seller).HasMaxLength(256);

		// ===== Числа/даты =====
		b.Property(x => x.Code); // уникальный индекс ниже
		b.Property(x => x.AreaTotal);
		b.Property(x => x.AreaLiving);
		b.Property(x => x.AreaKitchen);

		b.Property(x => x.Rooms);
		b.Property(x => x.Storey);
		b.Property(x => x.Storeys);

		b.Property(x => x.BuildingYear);
		b.Property(x => x.OverhaulYear);

		// Гео: явный тип double precision
		b.Property(x => x.Longitude).HasColumnType("double precision");
		b.Property(x => x.Latitude).HasColumnType("double precision");

		b.Property(x => x.HouseNumber);
		b.Property(x => x.ViewsCount);
		b.Property(x => x.Paid);

		b.Property(x => x.CreatedAt).HasConversion(dtoUtcConverter);
		b.Property(x => x.UpdatedAt).HasConversion(dtoUtcConverter);
		b.Property(x => x.RaiseDate).HasConversion(dtoUtcConverter);
		b.Property(x => x.NewAgainDate).HasConversion(dtoUtcConverter);

		// ===== Деньги (numeric) =====
		b.Property(x => x.PriceUsd).HasPrecision(18, 2);
		b.Property(x => x.PriceRub).HasPrecision(18, 2); // у вас историческое поле
		b.Property(x => x.PriceByn).HasPrecision(18, 2); // 933
		b.Property(x => x.PriceRubRus).HasPrecision(18, 2); // 643
		b.Property(x => x.PriceEur).HasPrecision(18, 2); // 978

		// ===== Коллекции строк → PostgreSQL text[] =====
		var listToArrayConverter = new ValueConverter<List<string>, string[]>(
			v => v.ToArray(),
			v => v.ToList());

		var listComparer = new ValueComparer<List<string>>(
			(x, y) => ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y)),
			x => x.Aggregate(0, (hash, s) => hash ^ (s.GetHashCode())),
			x => x.ToList()
		);

		b.Property(x => x.Images)
			.HasConversion(listToArrayConverter, listComparer)
			.HasColumnType("text[]");

		b.Property(x => x.ContactPhones)
			.HasConversion(listToArrayConverter, listComparer)
			.HasColumnType("text[]");

		b.Property(x => x.Appliances)
			.HasConversion(listToArrayConverter, listComparer)
			.HasColumnType("text[]");

		// ===== Индексы =====

		// Уникальность Code (если не NULL)
		// Важно: фильтр для PostgreSQL должен ссылаться на точное имя столбца в кавычках.
		// b.HasIndex(x => x.Code)
		// 	.IsUnique()
		// 	.HasFilter("\"code\" IS NOT NULL");

		// Полезные индексы (по ситуации) — раскомментируйте при необходимости:
		// b.HasIndex(x => x.PriceUsd);
		// b.HasIndex(x => x.PriceByn);
		// b.HasIndex(x => x.UpdatedAt);
		// b.HasIndex(x => x.CreatedAt);
		// b.HasIndex(x => x.TownName);
	}
}