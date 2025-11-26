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

        // Obtiene la lista completa de conductores
        [HttpGet]
        [Authorize(Roles = "Gerente")]
        public async Task<ActionResult<IEnumerable<ConductorDto>>> GetConductores()
        {
            var lista = await _conductorService.ObtenerListaConductores();
            return Ok(lista);
        }

        // Obtiene el porcentaje de combustible usado por un conductor
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

        // Registra el uso de un vehiculo por parte del conductor
        [HttpPost("registrar-uso")]
        [Authorize(Roles = "Conductor,Gerente")]
        public async Task<IActionResult> RegistrarUso([FromBody] RegistrarUsoDto dto)
        {
            // Obtiene el id del usuario desde el token
            var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(conductorId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            // Envia la informacion al servicio
            var resultado = await _conductorService.RegistrarUsoVehiculo(dto, conductorId);

            if (resultado.StartsWith("Error")) 
                return BadRequest(new { message = resultado });

            return Ok(new { message = resultado });
        }

        // Obtiene el reporte general de los conductores
        [HttpGet("reporte-general")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GetReporteGeneral()
        {
            var reporte = await _conductorService.ObtenerReporteGeneral();
            return Ok(reporte);
        }
    }
}
