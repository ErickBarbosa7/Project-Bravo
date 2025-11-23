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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterUserAsync(registerDto);

            if (!result.Success)
            {
                if (result.Message.Contains("uso")) 
                    return StatusCode(StatusCodes.Status409Conflict, new { message = result.Message });
                
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var tokenDto = await _authService.LoginUserAsync(loginDto);

            if (tokenDto == null)
            {
                return Unauthorized(new { message = "Credenciales invalidas" });
            }

            return Ok(tokenDto);
        }
    }
}