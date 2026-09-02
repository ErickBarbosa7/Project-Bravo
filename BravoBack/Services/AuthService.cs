using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravoBack.Services
{
    // Códigos de error para el registro de usuario. El controlador mapea
    // estos códigos a estatus HTTP concretos sin depender de texto.
    public enum RegisterError
    {
        None,
        EmailInUse,
        InvalidUserFields,
        InternalError
    }

    // Resultado estructurado del registro para evitar inferir el estatus
    // HTTP leyendo el texto del mensaje.
    public sealed record RegisterResult(
        bool Success,
        RegisterError Error = RegisterError.None,
        string Message = "");

    // Servicio que maneja registro, login y generacion de tokens
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        // Registro de usuario
        public async Task<RegisterResult> RegisterUserAsync(RegisterDto registerDto)
        {
            try
            {
                // Normalizar email antes de usarlo para reducir duplicados por formato
                var email = registerDto.Email.Trim().ToLowerInvariant();

                // Revisar si el correo ya existe
                var userExists = await _userManager.FindByEmailAsync(email);
                if (userExists != null)
                {
                    return new RegisterResult(false, RegisterError.EmailInUse, "El correo ya esta en uso");
                }

                // Construir el usuario (campos recortados)
                ApplicationUser user = new()
                {
                    Email = email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email,
                    FirstName = registerDto.FirstName.Trim(),
                    PaternalLastName = registerDto.PaternalLastName.Trim(),
                    MaternalLastName = registerDto.MaternalLastName?.Trim() ?? ""
                };

                // Guardar usuario en la base
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    LogIdentityErrors(result);
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new RegisterResult(false, RegisterError.InvalidUserFields, $"Error al crear usuario: {errors}");
                }

                // Crear roles si no existen
                await EnsureRoleExists("Gerente");
                await EnsureRoleExists("Conductor");

                // Asignar rol valido, si no, usar Conductor
                string roleToAssign = (registerDto.Role == "Gerente" || registerDto.Role == "Conductor")
                                      ? registerDto.Role
                                      : "Conductor";

                await _userManager.AddToRoleAsync(user, roleToAssign);

                return new RegisterResult(true, Message: "Usuario creado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de infraestructura al registrar usuario {Email}", registerDto.Email);
                return new RegisterResult(false, RegisterError.InternalError,
                    "Ocurrió un error interno al registrar el usuario. Inténtalo de nuevo.");
            }
        }

        // Login de usuario
        public async Task<UserTokenDto?> LoginUserAsync(LoginDto loginDto)
        {
            try
            {
                // Buscar usuario por correo (normalizado)
                var email = loginDto.Email.Trim().ToLowerInvariant();
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return null;

                // Validar password
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded) return null;

                // Obtener rol del usuario
                var userRoles = await _userManager.GetRolesAsync(user);

                // Crear token
                return GenerateJwtToken(user, userRoles.FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de infraestructura al iniciar sesión de {Email}", loginDto.Email);
                return null;
            }
        }

        // Devuelve la lista de usuarios registrados (uso administrativo).
        public async Task<IEnumerable<UserInfoDto>> GetUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var items = new List<UserInfoDto>(users.Count);

            foreach (var usr in users)
            {
                var roles = await _userManager.GetRolesAsync(usr);
                items.Add(new UserInfoDto
                {
                    Email = usr.Email ?? "",
                    FirstName = usr.FirstName,
                    PaternalLastName = usr.PaternalLastName,
                    MaternalLastName = usr.MaternalLastName,
                    Role = roles.FirstOrDefault() ?? ""
                });
            }

            return items;
        }

        // Crea el rol si aun no existe
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Loguea los errores devueltos por Identity con su codigo y descripcion,
        // para facilitar el diagnostico desde los logs del servidor.
        private void LogIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Identity error al crear usuario. Code={Code}, Description={Description}",
                    error.Code, error.Description);
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
