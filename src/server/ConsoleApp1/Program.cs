using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;

var baseDir = AppContext.BaseDirectory;
var envPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", ".env"));
if (!File.Exists(envPath))
{
	Console.Error.WriteLine($".env не найден: {envPath}");
	return 1;
}

Env.Load(envPath);

var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
	Console.Error.WriteLine("GROQ_API_KEY не задан.");
	return 1;
}

Console.WriteLine(apiKey);

var model = "llama-3.1-8b-instant";

using var http = new HttpClient();
http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var jsonOptions = new JsonSerializerOptions
{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// История чата (первая — системная)
var messages = new List<Message>
{
	new()
	{
		Role = "system",
		Content = "You are a concise, helpful assistant. Answer in Russian when the user writes in Russian."
	}
};

Console.WriteLine("Готово. Команды: /exit — выход, /reset — очистить контекст, /model <id> — сменить модель.");
Console.WriteLine($"Текущая модель: {model}");
Console.WriteLine();

while (true)
{
	Console.Write("> ");
	var input = Console.ReadLine();
	if (input is null) break;
	input = input.Trim();
	if (input.Length == 0) continue;

	// Команды управления
	if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

	if (input.Equals("/reset", StringComparison.OrdinalIgnoreCase))
	{
		messages.RemoveAll(m => m.Role != "system");
		Console.WriteLine("Контекст очищен.");
		continue;
	}

	if (input.StartsWith("/model ", StringComparison.OrdinalIgnoreCase))
	{
		var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 2)
		{
			model = parts[1].Trim();
			Console.WriteLine($"Модель сменена на: {model}");
		}
		else
			Console.WriteLine("Использование: /model <model-id>");

		continue;
	}

	// Добавляем сообщение пользователя
	messages.Add(new Message { Role = "user", Content = input });

	// Формируем запрос
	var request = new ChatRequest
	{
		Model = model, Messages = messages, Temperature = 0.2
		// MaxTokens = 512 // при необходимости
	};

	// Отправляем
	using var resp = await http.PostAsJsonAsync("chat/completions", request, jsonOptions);
	var body = await resp.Content.ReadAsStringAsync();

	if (!resp.IsSuccessStatusCode)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"HTTP {(int)resp.StatusCode}: {body}");
		Console.ResetColor();
		// Не добавляем assistant-ответ в историю при ошибке
		continue;
	}

	var data = JsonSerializer.Deserialize<ChatResponse>(body, jsonOptions);
	var text = data?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

	if (string.IsNullOrWhiteSpace(text))
	{
		Console.WriteLine("(пустой ответ)");
		continue;
	}

	// Печатаем и сохраняем ответ ассистента в контекст
	Console.WriteLine(text);
	messages.Add(new Message { Role = "assistant", Content = text });
}

return 0;

// ===== DTOs =====
public sealed class ChatRequest
{
	[JsonPropertyName("model")] public string? Model { get; set; }
	[JsonPropertyName("messages")] public List<Message> Messages { get; set; } = new();
	[JsonPropertyName("temperature")] public double? Temperature { get; set; }
	[JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }
	[JsonPropertyName("stream")] public bool? Stream { get; set; } // не используем здесь, но оставим поле
}

public sealed class Message
{
	[JsonPropertyName("role")] public string Role { get; set; } = "";
	[JsonPropertyName("content")] public string Content { get; set; } = "";
}

public sealed class ChatResponse
{
	[JsonPropertyName("id")] public string? Id { get; set; }
	[JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = new();
}

public sealed class Choice
{
	[JsonPropertyName("index")] public int Index { get; set; }
	[JsonPropertyName("message")] public Message? Message { get; set; }
	[JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
}