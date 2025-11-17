using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;

// Lo que esperamos recibir cuando un usuario se registra
public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string PaternalLastName { get; set; } = null!;

    public string MaternalLastName { get; set; } = null!; 

    [Required]
    public string Role { get; set; } = null!;
}


public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

// Lo que le devolveremos al usuario si el login es exitoso
public class UserTokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}

//devolver info completa del usuario
public class UserInfoDto
{
    public string FirstName { get; set; } = null!;
    public string PaternalLastName { get; set; } = null!;
    public string MaternalLastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string FullName => $"{FirstName} {PaternalLastName} {MaternalLastName}".Trim();
}
