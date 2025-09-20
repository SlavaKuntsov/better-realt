using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Utilities.Services;

public class EmailQueueService(
	IDistributedCache cache,
	ILogger<EmailQueueService> logger,
	IConnectionMultiplexer? redis = null) : IEmailQueueService
{
	private readonly IDatabase? _db = redis?.GetDatabase();
	private const string QueueKey = "email_queue";

	public async Task EnqueueEmailAsync(string recipient, string subject, string body)
	{
		var message = new EmailMessage(recipient, subject, body);
		var json = JsonSerializer.Serialize(message);

		if (_db is not null)
		{
			await _db.ListRightPushAsync(QueueKey, json);
		}
		else
		{
			var existing = await cache.GetStringAsync(QueueKey);
			var list = string.IsNullOrEmpty(existing)
				? new List<string>()
				: JsonSerializer.Deserialize<List<string>>(existing!) ?? new List<string>();
			list.Add(json);
			await cache.SetStringAsync(QueueKey, JsonSerializer.Serialize(list));
		}

		logger.LogInformation("Email queued for {Recipient}", recipient);
	}
}