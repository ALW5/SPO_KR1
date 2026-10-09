using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOne.Application.Auth.Login;
using ProjectOne.Application.Auth.Register;
using ProjectOne.Extensions;

namespace ProjectOne.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(
        [FromServices] RegisterHandler handler, [FromBody] RegisterRequest request)
    {
        var result = await handler.RegisterAsync(request);
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        SetTokenCookie(result.Value);
        return Ok();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromServices] LoginHandler handler, [FromBody] LoginRequest request)
    {
        var result = await handler.LoginAsync(request);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        SetTokenCookie(result.Value);
        return Ok();
    }
    
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return Ok();
    }
    
    [HttpGet("me")]
    [Authorize]
    public ActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new { id, email, fullName = name, role });
    }

    private void SetTokenCookie(string token)
    {
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(120)
        });
    }
}