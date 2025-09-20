namespace Utilities.Services;

public interface IEmailQueueService
{
	Task EnqueueEmailAsync(string recipient, string subject, string body);
}