using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Utilities.Services;

public class EmailQueueBackgroundService(
	IEmailService emailService,
	IDistributedCache cache,
	ILogger<EmailQueueBackgroundService> logger,
	IConnectionMultiplexer? redis = null) : BackgroundService
{
	private readonly IDatabase? _db = redis?.GetDatabase();
	private const string QueueKey = "email_queue";

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				EmailMessage? message = null;

				if (_db is not null)
				{
					var value = await _db.ListLeftPopAsync(QueueKey);
					if (value.HasValue)
					{
						message = JsonSerializer.Deserialize<EmailMessage>(value!);
					}
				}
				else
				{
					var listJson = await cache.GetStringAsync(QueueKey, token: stoppingToken);
					if (!string.IsNullOrEmpty(listJson))
					{
						var list = JsonSerializer.Deserialize<List<string>>(listJson!) ?? new List<string>();
						if (list.Count > 0)
						{
							var first = list[0];
							list.RemoveAt(0);
							await cache.SetStringAsync(QueueKey, JsonSerializer.Serialize(list), token: stoppingToken);
							message = JsonSerializer.Deserialize<EmailMessage>(first);
						}
					}
				}

				if (message is not null)
				{
					await emailService.SendEmailAsync(message.Recipient, message.Subject, message.Body);
				}
				else
				{
					await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing email queue");
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}
}