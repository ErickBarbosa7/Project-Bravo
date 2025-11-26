using BravoBack.DTOs;
using BravoBack.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BravoBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiculosController : ControllerBase
    {
        private readonly VehiculoService _vehiculoService;

        public VehiculosController(VehiculoService vehiculoService)
        {
            _vehiculoService = vehiculoService;
        }

        // Obtiene la lista de vehiculos
        [HttpGet]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetVehiculos()
        {
            var vehiculos = await _vehiculoService.ObtenerTodos();
            return Ok(vehiculos);
        }

        // Obtiene un vehiculo por id
        [HttpGet("{id}")]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
        {
            var vehiculo = await _vehiculoService.ObtenerPorId(id);

            if (vehiculo == null)
                return NotFound();

            return Ok(vehiculo);
        }

        // Crea un vehiculo nuevo
        [HttpPost]
        [Authorize(Roles = "Gerente")]
        public async Task<ActionResult<VehiculoDto>> CreateVehiculo(
            [FromBody] CreateVehiculoDto createDto,
            [FromServices] IValidator<CreateVehiculoDto> validator)
        {
            // Validacion de datos
            var validationResult = await validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            // Crear vehiculo
            var nuevoVehiculo = await _vehiculoService.CrearVehiculo(createDto);

            return CreatedAtAction(nameof(GetVehiculo), new { id = nuevoVehiculo.Id }, nuevoVehiculo);
        }

        // Actualiza un vehiculo
        [HttpPut("{id}")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> UpdateVehiculo(
            int id,
            [FromBody] UpdateVehiculoDto updateDto,
            [FromServices] IValidator<UpdateVehiculoDto> validator)
        {
            updateDto.Id = id;

            // Validacion
            var validationResult = await validator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            // Actualizar vehiculo
            var vehiculoActualizado = await _vehiculoService.ActualizarVehiculo(id, updateDto);

            if (vehiculoActualizado == null)
                return NotFound();

            return Ok(vehiculoActualizado);
        }

        // Elimina un vehiculo
        [HttpDelete("{id}")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> DeleteVehiculo(int id)
        {
            var eliminado = await _vehiculoService.EliminarVehiculo(id);

            if (!eliminado)
                return NotFound();

            return Ok(new { message = "Vehiculo eliminado" });
        }

        // Obtiene el estado de mantenimiento
        [HttpGet("{id}/estatus-servicio")]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<ReporteMantenimientoDto>> GetEstatusServicio(int id)
        {
            var reporte = await _vehiculoService.ObtenerEstadoServicio(id);

            if (reporte.Estatus == Models.Enums.EstatusMantenimiento.Desconocido)
                return NotFound(new { message = reporte.Mensaje });

            return Ok(reporte);
        }

        // Simula un pago de servicio
        [HttpPost("simular-pago")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> PagarServicio([FromBody] PagoServicioDTO pagoDto)
        {
            var resultado = await _vehiculoService.SimularPagoServicio(pagoDto);

            if (resultado.StartsWith("Error"))
                return BadRequest(new { message = resultado });

            return Ok(new { message = resultado });
        }

        // Genera la proyeccion de gastos
        [HttpGet("proyeccion-gastos")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GetProyeccionGastos()
        {
            var reporte = await _vehiculoService.CalcularProyeccionMensual();
            return Ok(reporte);
        }

        // Envia un vehiculo al taller
        [HttpPut("{id}/enviar-taller")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> SendToWorkshop(int id)
        {
            var result = await _vehiculoService.EnviarATaller(id);

            if (!result)
                return NotFound("Vehiculo no encontrado");

            return Ok(new { message = "Vehiculo enviado a taller" });
        }

        // Recomienda vehiculos para un viaje
        [HttpGet("recomendar")]
        [Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<RecomendacionVehiculoDto>>> GetRecomendacion([FromQuery] int distancia)
        {
            if (distancia <= 0) return BadRequest("La distancia debe ser mayor a 0 km.");

            var resultados = await _vehiculoService.RecomendarVehiculos(distancia);
            return Ok(resultados);
        }
    }
}
