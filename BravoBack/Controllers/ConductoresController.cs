using BravoBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using BravoBack.DTOs;

namespace BravoBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConductoresController : ControllerBase
    {
        private readonly ConductorService _conductorService;

        public ConductoresController(ConductorService conductorService)
        {
            _conductorService = conductorService;
        }
        // GET: api/conductores
        [HttpGet]
        [Authorize(Roles = "Gerente")] 
        public async Task<ActionResult<IEnumerable<ConductorDto>>> GetConductores()
        {
            var lista = await _conductorService.ObtenerListaConductores();
            return Ok(lista);
        }
        // GET: api/conductores/{id}/combustible
        [HttpGet("{id}/combustible")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GetRendimientoCombustible(string id)
        {
            var resultado = await _conductorService.CalcularPorcentajeCombustible(id);

            var jsonStr = System.Text.Json.JsonSerializer.Serialize(resultado);
            if (jsonStr.Contains("Conductor no encontrado"))
            {
                return NotFound(resultado);
            }

            return Ok(resultado);
        }
        // POST: api/conductores/registrar-uso
        [HttpPost("registrar-uso")]
        [Authorize(Roles = "Conductor,Gerente")] // Ambos pueden manejar
        public async Task<IActionResult> RegistrarUso([FromBody] RegistrarUsoDto dto)
        {
            // 1. Extraer el ID del usuario desde el Token (Claim)
            // Esto asegura que quien hace la petición es quien queda registrado
            var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(conductorId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            // 2. Llamar al servicio
            var resultado = await _conductorService.RegistrarUsoVehiculo(dto, conductorId);

            if (resultado.StartsWith("Error")) return BadRequest(new { message = resultado });

            return Ok(new { message = resultado });
        }

        // GET: api/conductores/reporte-general
        [HttpGet("reporte-general")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GetReporteGeneral()
        {
            var reporte = await _conductorService.ObtenerReporteGeneral();
            return Ok(reporte);
        }
    }
}