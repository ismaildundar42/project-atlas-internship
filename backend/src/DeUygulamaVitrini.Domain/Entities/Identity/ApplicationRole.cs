using Microsoft.AspNetCore.Identity;

namespace DeUygulamaVitrini.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
