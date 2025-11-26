using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravoBack.Services
{
    // Servicio que maneja registro, login y generacion de tokens
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // Registro de usuario
        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterDto registerDto)
        {
            // Revisar si el correo ya existe
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return (false, "El correo ya esta en uso");
            }

            // Construir el usuario
            ApplicationUser user = new()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                PaternalLastName = registerDto.PaternalLastName,
                MaternalLastName = registerDto.MaternalLastName
            };

            // Guardar usuario en la base
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Error al crear usuario: {errors}");
            }

            // Crear roles si no existen
            await EnsureRoleExists("Gerente");
            await EnsureRoleExists("Conductor");

            // Asignar rol valido, si no, usar Conductor
            string roleToAssign = (registerDto.Role == "Gerente" || registerDto.Role == "Conductor")
                                  ? registerDto.Role
                                  : "Conductor";

            await _userManager.AddToRoleAsync(user, roleToAssign);

            return (true, "Usuario creado");
        }

        // Login de usuario
        public async Task<UserTokenDto?> LoginUserAsync(LoginDto loginDto)
        {
            // Buscar usuario por correo
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return null;

            // Validar password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return null;

            // Obtener rol del usuario
            var userRoles = await _userManager.GetRolesAsync(user);

            // Crear token
            return GenerateJwtToken(user, userRoles.FirstOrDefault());
        }

        // Crea el rol si aun no existe
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Genera el token JWT
        private UserTokenDto GenerateJwtToken(ApplicationUser user, string? role)
        {
            // Lista de claims del token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar claim de rol
            if (!string.IsNullOrEmpty(role))
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Leer la llave del appsettings
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
                throw new Exception("Falta la llave JWT en appsettings.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Definir expiracion
            var expiration = DateTime.UtcNow.AddHours(8);

            // Construir token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Regresar info para el front
            return new UserTokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                Email = user.Email,
                Role = role ?? "",
                FirstName = user.FirstName,
                PaternalLastName = user.PaternalLastName,
                MaternalLastName = user.MaternalLastName ?? ""
            };
        }
    }
}
