using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;

// Lo que esperamos recibir cuando un usuario se registra
public class RegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class LoginDto
{
    public string Email { get; set; } = null!;
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