using System.Collections;

namespace Common.Contracts;

public record ApiResponse<T>(
	T? Data,
	string? Message = null)
{
	public int? Total =>
		Data is IEnumerable e ? e.Cast<object>().Count() : null;
}