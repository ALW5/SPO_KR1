using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using ProjectOne.Application.Identity;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
namespace ProjectOne.Application.Auth.Register;

public record RegisterRequest(string email, string password, string fullName);

public class RegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<string, Error>> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.email,
            Email = request.email,
            FullName = request.fullName
        };

        var result = await _userManager.CreateAsync(user, request.password);

        if (!result.Succeeded)
            return Error.Validation(
                "register.failed",
                string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, Roles.Client); // по умолчанию — Client

        var token = await _tokenService.GenerateTokenAsync(user);
        return token;
    }
}