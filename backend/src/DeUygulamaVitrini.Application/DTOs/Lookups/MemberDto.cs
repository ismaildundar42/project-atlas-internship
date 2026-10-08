namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class MemberDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? ProjectRole { get; set; }
}
