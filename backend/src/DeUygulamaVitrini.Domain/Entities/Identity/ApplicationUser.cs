using Microsoft.AspNetCore.Identity;

namespace DeUygulamaVitrini.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool CanCreateProjects { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
