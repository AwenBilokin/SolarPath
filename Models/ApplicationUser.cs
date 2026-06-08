using Microsoft.AspNetCore.Identity;

namespace SolarPath.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string FullName => $"{FirstName} {LastName}";
}
