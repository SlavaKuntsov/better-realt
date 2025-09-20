namespace Minios;

public class MinioOptions
{
	public string Endpoint { get; set; } = string.Empty;
	public string AccessKey { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	public bool UseSsl { get; set; }
	public string DefaultBucket { get; set; } = "default";
	public string? ExternalEndpoint { get; set; }
}