using Flatly.Core.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flatly.Persistance.DataAccess.Configurations;

public class RealEstateConfiguration: IEntityTypeConfiguration<RealEstateModel>
{
	public void Configure(EntityTypeBuilder<RealEstateModel> builder)
	{
		builder.HasKey(c => c.Id);

		// builder.Property(c => c.Title)
		// 	.IsRequired()
		// 	.HasMaxLength(100);
		//
		// builder.HasOne(c => c.ParentCategory)
		// 	.WithMany(c => c.SubCategories)
		// 	.HasForeignKey(c => c.ParentCategoryId)
		// 	.OnDelete(DeleteBehavior.Cascade);
	}
}