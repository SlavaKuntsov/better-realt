using Flatly.Core.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Flatly.Core.Abstractions.Data;

public interface IApplicationDbContext
{
	DbSet<RealEstateModel> RealEstates { get; set; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	
	ChangeTracker ChangeTracker { get; }  
}