using Microsoft.AspNetCore.Identity;

namespace ProjectOne.Application.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}