using BravoBack.DTOs;
using BravoBack.Services; 
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
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterUserAsync(registerDto);

            if (!result.Success)
            {
                // Si el mensaje indica que el correo ya esta en uso, devuelve conflicto
                if (result.Message.Contains("uso")) 
                    return StatusCode(StatusCodes.Status409Conflict, new { message = result.Message });
                
                // Cualquier otro error es una solicitud invalida
                return BadRequest(new { message = result.Message });
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
    }
}
