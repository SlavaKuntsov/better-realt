namespace Utilities.Services;

public interface ICookieService
{
	void DeleteRefreshToken();
	string GetRefreshToken();
}