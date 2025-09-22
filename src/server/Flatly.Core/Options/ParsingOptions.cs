// Flatly.Core/Options/ParsingOptions.cs

namespace Flatly.Core.Options;

public sealed class ParsingOptions
{
	public List<string> Links { get; init; } = [];

	// Шаблон URL для карточки объекта. Пример:
	// "https://realt.by/rent-flat-for-long/object/{code}/"
	public string ObjectUrlTemplate { get; init; } = "https://realt.by/rent-flat-for-long/object/{code}/";

	// Параллелизм
	public int PageConcurrency { get; init; } = 6; // страницы со списками
	public int DetailConcurrency { get; init; } = 32; // карточки объектов

	// Батчи для EF
	public int SaveBatchSize { get; init; } = 400;

	// Дросселирование запросов к карточкам (нестрогое)
	public int? ThrottleMinMs { get; init; } = 50;
	public int? ThrottleMaxMs { get; init; } = 150;

	// Пропуск апдейтов, если UpdatedAt не изменился (если в БД хранится UpdatedAt)
	public bool SkipUnchanged { get; init; } = true;
}