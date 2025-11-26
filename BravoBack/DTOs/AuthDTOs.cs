using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;


// DTO que representa lo que esperamos recibir cuando un usuario se registras
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
    public string? MaternalLastName { get; set; } = null!;

    // Rol del usuario solo puede ser Gerente o Conductor
    [Required]
    [RegularExpression("Gerente|Conductor", ErrorMessage = "Rol inválido.")]
    public string Role { get; set; } = null!;
}


// DTO para el inicio de sesion (login)
public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}


// DTO que le devolveremos al usuario después de un login correcto
public class UserTokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string PaternalLastName { get; set; } = string.Empty;
    public string MaternalLastName { get; set; } = string.Empty;
}


// DTO para devolver información mas completa del usuario
public class UserInfoDto
{
    public string FirstName { get; set; } = null!;
    public string PaternalLastName { get; set; } = null!;
    public string? MaternalLastName { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;

    // Nombre completo construido
    public string FullName => string.Join(" ",
        new[] { FirstName, PaternalLastName, MaternalLastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
    );
}
