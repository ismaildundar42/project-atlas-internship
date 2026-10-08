namespace DeUygulamaVitrini.Application.DTOs.Auth;

public class AuthUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public bool IsAdmin { get; set; }
    public bool CanCreateProjects { get; set; }
    public List<string> Roles { get; set; } = new();
}
