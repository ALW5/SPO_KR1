using ProjectOne.Application.Identity;

namespace ProjectOne.Application.Auth;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}