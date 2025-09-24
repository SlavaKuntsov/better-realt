using System.Collections;

namespace Common.Contracts;

public record ApiResponse<T>(
	T? Data,
	byte Limit = 10,
	byte Offset = 1,
	string? Message = null,
	int? Total = null)
{
	public int? Total { get; init; } =
		Total ?? (Data is IEnumerable e ? e.Cast<object>().Count() : null);
}