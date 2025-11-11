using Microsoft.AspNetCore.Identity;
namespace BravoBack.Models; // <== Namespace actualizado
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}