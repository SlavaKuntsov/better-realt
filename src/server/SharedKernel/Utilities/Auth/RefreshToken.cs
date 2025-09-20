namespace Utilities.Auth;

public sealed class RefreshToken
{
	public Guid Id { get; set; }
	public Guid? UserId { get; set; }
	public string Token { get; set; }
	public System.DateTime ExpiresAt { get; set; }
	public System.DateTime CreatedAt { get; set; }
	public bool IsRevoked { get; set; }

	// public virtual User User { get; set; } = null!;

	public RefreshToken() { }

	public RefreshToken(
		Guid userId,
		string token,
		int refreshTokenExpirationDays)
	{
		Id = Guid.NewGuid();
		Token = token;
		ExpiresAt = System.DateTime.UtcNow.Add(TimeSpan.FromDays(refreshTokenExpirationDays));
		CreatedAt = System.DateTime.UtcNow;
		IsRevoked = false;
		UserId = userId;
	}
}