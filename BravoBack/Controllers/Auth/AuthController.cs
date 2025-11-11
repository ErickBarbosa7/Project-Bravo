using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravoBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context; // Para la lógica de roles

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        // 1. Verificar si el usuario ya existe
        var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status409Conflict, 
                new { message = "Error: El correo electrónico ya está en uso." });
        }

        // 2. Crear el nuevo ApplicationUser
        ApplicationUser user = new()
        {
            Email = registerDto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerDto.Email, // Usamos el email como username
            FullName = registerDto.FullName
        };

        // 3. Crear el usuario en la BD (tabla AspNetUsers)
        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error: La creación del usuario falló.", errors = result.Errors });
        }

        // 4. Asegurarnos de que los roles existan (Gerente, Conductor)
        await EnsureRoleExists("Gerente");
        await EnsureRoleExists("Conductor");

        // 5. Asignar el rol al nuevo usuario
        if (registerDto.Role == "Gerente" || registerDto.Role == "Conductor")
        {
            await _userManager.AddToRoleAsync(user, registerDto.Role);
        }
        else
        {
            // Por defecto, si el rol no es válido, asignamos Conductor
            await _userManager.AddToRoleAsync(user, "Conductor");
        }

        return Ok(new { message = "Usuario creado exitosamente." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        // 1. Buscar al usuario
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // 2. Verificar la contraseña
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // 3. Si todo es correcto, generar el token JWT
        var userRoles = await _userManager.GetRolesAsync(user);
        var userTokenDto = GenerateJwtToken(user, userRoles.FirstOrDefault());

        return Ok(userTokenDto);
    }


    // --- Métodos de Ayuda ---

    // Método para crear el token JWT
    private UserTokenDto GenerateJwtToken(ApplicationUser user, string? role)
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), // Guardamos el ID del usuario
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (role != null)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role)); // Guardamos el Rol
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var expiration = DateTime.UtcNow.AddHours(8); // El token dura 8 horas

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: expiration,
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new UserTokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            Email = user.Email,
            Role = role ?? ""
        };
    }

    // Método para crear roles si no existen
    private async Task EnsureRoleExists(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}