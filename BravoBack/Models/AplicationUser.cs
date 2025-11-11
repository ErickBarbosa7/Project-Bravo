using Microsoft.AspNetCore.Identity;
namespace BravoBack.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}