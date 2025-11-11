using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;

// Lo que esperamos recibir cuando un usuario se registra
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!; // "Gerente" o "Conductor"
}

// Lo que esperamos recibir cuando un usuario inicia sesión
public class LoginDto
{
    [Required]
    [EmailAddress]
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