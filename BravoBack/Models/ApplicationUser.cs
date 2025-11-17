using Microsoft.AspNetCore.Identity;
namespace BravoBack.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string PaternalLastName { get; set; } = string.Empty;
    public string MaternalLastName { get; set; } = string.Empty;

}