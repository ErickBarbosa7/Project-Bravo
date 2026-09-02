using BravoBack.DTOs;
using BravoBack.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BravoBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Registra un nuevo usuario en el sistema
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto registerDto,
            [FromServices] IValidator<RegisterDto> validator)
        {
            // Validacion de datos (mismas reglas que en RegisterValidator)
            var validationResult = await validator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var result = await _authService.RegisterUserAsync(registerDto);

            if (!result.Success)
            {
                // Mapea el código de error a un estatus HTTP concreto
                return result.Error switch
                {
                    RegisterError.EmailInUse => StatusCode(StatusCodes.Status409Conflict, new { message = result.Message }),
                    RegisterError.InternalError => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Message }),
                    _ => BadRequest(new { message = result.Message })
                };
            }

            return Ok(new { message = result.Message });
        }

        // Inicia sesion y devuelve el token JWT si las credenciales son validas
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var tokenDto = await _authService.LoginUserAsync(loginDto);

            // Si no se genera token, las credenciales no coinciden
            if (tokenDto == null)
            {
                return Unauthorized(new { message = "Credenciales invalidas" });
            }

            return Ok(tokenDto);
        }

        // Lista los usuarios registrados (solo acceso para Gerente)
        [Authorize(Roles = "Gerente")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetUsersAsync();
            return Ok(users);
        }
    }
}
