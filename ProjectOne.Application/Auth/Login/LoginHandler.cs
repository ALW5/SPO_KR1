using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using ProjectOne.Application.Identity;
using ProjectOne.Domain.Common;

namespace ProjectOne.Application.Auth.Login;

public record LoginRequest(string email, string password);

public class LoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<Result<string, Error>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.email);
        if (user is null)
            return Error.NotFound("user.not.found", "Invalid credentials");

        var checkResult = await _signInManager.CheckPasswordSignInAsync(user, request.password, false);
        if (!checkResult.Succeeded)
            return Error.Validation("invalid.credentials", "Invalid credentials");

        var token = await _tokenService.GenerateTokenAsync(user);
        return token;
    }
}