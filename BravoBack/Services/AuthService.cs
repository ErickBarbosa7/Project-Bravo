using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravoBack.Services
{
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

        // 1. REGISTRO DE USUARIO
        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterDto registerDto)
        {
            // Verificar si ya existe
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return (false, "El correo electronico ya esta en uso");
            }

            // Crear objeto usuario
            ApplicationUser user = new()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                PaternalLastName = registerDto.PaternalLastName,
                MaternalLastName = registerDto.MaternalLastName
            };

            // Guardar en BD
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Error al crear usuario: {errors}");
            }

            // Asignar Rol
            await EnsureRoleExists("Gerente");
            await EnsureRoleExists("Conductor");

            string roleToAssign = (registerDto.Role == "Gerente" || registerDto.Role == "Conductor") 
                                  ? registerDto.Role 
                                  : "Conductor";

            await _userManager.AddToRoleAsync(user, roleToAssign);

            return (true, "Usuario creado exitosamente.");
        }

        // LOGIN
        public async Task<UserTokenDto?> LoginUserAsync(LoginDto loginDto)
        {
            // Buscar usuario
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return null;

            // Verificar contraseña
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return null;

            // Generar Token
            var userRoles = await _userManager.GetRolesAsync(user);
            return GenerateJwtToken(user, userRoles.FirstOrDefault());
        }

        // MÉTODOS PRIVADOS
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private UserTokenDto GenerateJwtToken(ApplicationUser user, string? role)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (!string.IsNullOrEmpty(role))
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key)) throw new Exception("JWT Key no está configurada en appsettings.");

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var expiration = DateTime.UtcNow.AddHours(8);

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
    }
}