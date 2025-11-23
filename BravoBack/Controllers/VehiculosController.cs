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

        // GET: api/vehiculos
        [HttpGet]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetVehiculos()
        {
            var vehiculos = await _vehiculoService.ObtenerTodos();
            return Ok(vehiculos);
        }

        // GET: api/vehiculos/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
        {
            var vehiculo = await _vehiculoService.ObtenerPorId(id);

            if (vehiculo == null)
                return NotFound();

            return Ok(vehiculo);
        }

        // POST: api/vehiculos
        [HttpPost]
        [Authorize(Roles = "Gerente")]
        public async Task<ActionResult<VehiculoDto>> CreateVehiculo(
            [FromBody] CreateVehiculoDto createDto,
            [FromServices] IValidator<CreateVehiculoDto> validator)
        {
            // 1. Validación de entrada (Responsabilidad del Controller/Middleware)
            var validationResult = await validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            // 2. Lógica de negocio (Responsabilidad del Service)
            var nuevoVehiculo = await _vehiculoService.CrearVehiculo(createDto);

            return CreatedAtAction(nameof(GetVehiculo), new { id = nuevoVehiculo.Id }, nuevoVehiculo);
        }

        // PUT: api/vehiculos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> UpdateVehiculo(
            int id,
            [FromBody] UpdateVehiculoDto updateDto,
            [FromServices] IValidator<UpdateVehiculoDto> validator)
        {
            updateDto.Id = id;

            // 1. Validación
            var validationResult = await validator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            // 2. Lógica de negocio
            var vehiculoActualizado = await _vehiculoService.ActualizarVehiculo(id, updateDto);

            if (vehiculoActualizado == null)
                return NotFound();

            return Ok(vehiculoActualizado);
        }

        // DELETE: api/vehiculos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> DeleteVehiculo(int id)
        {
            var eliminado = await _vehiculoService.EliminarVehiculo(id);

            if (!eliminado)
                return NotFound();

            return Ok(new { message = "Vehículo eliminado correctamente" });
        }


        // GET: api/vehiculos/{id}/estatus-servicio
        [HttpGet("{id}/estatus-servicio")]
        [Authorize(Roles = "Gerente,Conductor")]
        public async Task<ActionResult<ReporteMantenimientoDto>> GetEstatusServicio(int id)
        {
            var reporte = await _vehiculoService.ObtenerEstadoServicio(id);

            if (reporte.Estatus == Models.Enums.EstatusMantenimiento.Desconocido)
                return NotFound(new { message = reporte.Mensaje });

            return Ok(reporte);
        }

        [HttpPost("simular-pago")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> PagarServicio([FromBody] PagoServicioDTO pagoDto)
        {
            var resultado = await _vehiculoService.SimularPagoServicio(pagoDto);
            if (resultado.StartsWith("Error")) return BadRequest(new { message = resultado });

            return Ok(new { message = resultado });
        }
        // GET: api/vehiculos/proyeccion-gastos
        [HttpGet("proyeccion-gastos")]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GetProyeccionGastos()
        {
            var reporte = await _vehiculoService.CalcularProyeccionMensual();
            return Ok(reporte);
        }
    }
}