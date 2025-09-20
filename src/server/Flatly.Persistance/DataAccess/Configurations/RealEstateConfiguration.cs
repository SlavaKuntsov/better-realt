using Flatly.Core.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Flatly.Persistance.DataAccess.Configurations;

public class RealEstateConfiguration: IEntityTypeConfiguration<RealEstateModel>
{
public void Configure(EntityTypeBuilder<RealEstateModel> b)
    {
        b.ToTable("real_estates");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        
        var dtoUtcConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
	        v => v.HasValue ? v.Value.ToUniversalTime() : v,
	        v => v // читаем как есть
        );

        // Тексты
        b.Property(x => x.Title).HasMaxLength(256);
        b.Property(x => x.Description).HasColumnType("text");
        b.Property(x => x.Headline).HasMaxLength(2000);
        b.Property(x => x.Address).HasMaxLength(512);
        b.Property(x => x.ImageUrl).HasMaxLength(1024);
        b.Property(x => x.ContactName).HasMaxLength(256);
        b.Property(x => x.ContactEmail).HasMaxLength(256);

        // Числа/даты
        b.Property(x => x.Code);
        b.Property(x => x.AreaTotal);
        b.Property(x => x.AreaLiving);
        b.Property(x => x.Rooms);
        b.Property(x => x.Storey);
        b.Property(x => x.Storeys);
        b.Property(x => x.CreatedAt).HasConversion(dtoUtcConverter);
        b.Property(x => x.UpdatedAt).HasConversion(dtoUtcConverter);

        // Деньги (numeric)
        b.Property(x => x.PriceUsd).HasPrecision(18, 2);
        b.Property(x => x.PriceRub).HasPrecision(18, 2);

        // Коллекции строк -> PostgreSQL text[]
        var listToArrayConverter = new ValueConverter<List<string>, string[]>(
	        v => v.ToArray(),
	        v => v.ToList());

        var listComparer = new ValueComparer<List<string>>(
	        (x, y) => x == y || (x != null && y != null && x.SequenceEqual(y)),
	        x => x.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
	        x => x.ToList());
        
        b.Property(x => x.Images)
            .HasConversion(listToArrayConverter, listComparer)
            .HasColumnType("text[]");

        b.Property(x => x.ContactPhones)
            .HasConversion(listToArrayConverter, listComparer)
            .HasColumnType("text[]");

        // b.HasIndex(x => x.Code);
        // b.HasIndex(x => x.PriceUsd);
        // b.HasIndex(x => x.PriceRub);
        // b.HasIndex(x => x.CreatedAt);
    }
}