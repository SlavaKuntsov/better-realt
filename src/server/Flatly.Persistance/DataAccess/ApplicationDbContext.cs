using System.Reflection;
using Flatly.Core.Abstractions.Data;
using Flatly.Core.RealEstate;
using Microsoft.EntityFrameworkCore;

namespace Flatly.Persistance.DataAccess;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: DbContext(options), IApplicationDbContext
{
	public DbSet<RealEstateModel> RealEstates { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		base.OnModelCreating(modelBuilder);
	}
}