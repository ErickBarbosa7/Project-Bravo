using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Authorization; // ¡Importante!
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // <== ¡PROTECCIÓN! Solo usuarios logueados pueden tocar esto
public class VehiculosController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiculosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/vehiculos
    // Obtener TODOS los vehículos
    [HttpGet]
    [Authorize(Roles = "Gerente,Conductor")] // Ambos pueden ver la lista
    public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetVehiculos()
    {
        var vehiculos = await _context.Vehiculos
            .Select(v => new VehiculoDto
            {
                Id = v.Id,
                Placa = v.Placa,
                Nombre = v.Nombre,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Año = v.Año,
                FotoUrl = v.FotoUrl,
                KilometrajeActual = v.KilometrajeActual,
                Estado = v.Estado.ToString(), // Convertir Enum a string
                IntervaloServicioKm = v.IntervaloServicioKm,
                SiguienteServicioKm = v.SiguienteServicioKm
            })
            .ToListAsync();

        return Ok(vehiculos);
    }

    // GET: api/vehiculos/5
    // Obtener UN vehículo por ID
    [HttpGet("{id}")]
    [Authorize(Roles = "Gerente,Conductor")]
    public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
    {
        var v = await _context.Vehiculos.FindAsync(id);

        if (v == null)
        {
            return NotFound();
        }

        // Mapeo simple
        var vehiculoDto = new VehiculoDto
        {
            Id = v.Id,
            Placa = v.Placa,
            Nombre = v.Nombre,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Año = v.Año,
            FotoUrl = v.FotoUrl,
            KilometrajeActual = v.KilometrajeActual,
            Estado = v.Estado.ToString(),
            IntervaloServicioKm = v.IntervaloServicioKm,
            SiguienteServicioKm = v.SiguienteServicioKm
        };

        return Ok(vehiculoDto);
    }

    // POST: api/vehiculos
    // Crear un nuevo vehículo
    [HttpPost]
    [Authorize(Roles = "Gerente")] // ¡Solo Gerentes!
    public async Task<ActionResult<VehiculoDto>> CreateVehiculo([FromBody] CreateVehiculoDto createDto)
    {
        // Validar si la placa ya existe
        if (await _context.Vehiculos.AnyAsync(v => v.Placa == createDto.Placa))
        {
            return Conflict(new { message = "La placa ya está registrada." });
        }

        var vehiculo = new Vehiculo
        {
            Placa = createDto.Placa,
            Nombre = createDto.Nombre,
            Marca = createDto.Marca,
            Modelo = createDto.Modelo,
            Año = createDto.Año,
            KilometrajeActual = createDto.KilometrajeActual,
            IntervaloServicioKm = createDto.IntervaloServicioKm,
            // Lógica de negocio: El primer servicio es el kilometraje + intervalo
            SiguienteServicioKm = createDto.KilometrajeActual + createDto.IntervaloServicioKm,
            Estado = EstadoVehiculo.Disponible // Nuevo vehículo siempre disponible
        };

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        // Devolvemos el DTO completo del vehículo recién creado
        return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.Id }, vehiculo);
    }
    
    // PUT api/vehiculos/5

    [HttpPut("{id}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> UpdateVehiculo(int id, [FromBody] CreateVehiculoDto updateDto)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return NotFound();
        }

        // Actualizar campos
        vehiculo.Placa = updateDto.Placa;
        vehiculo.Nombre = updateDto.Nombre;
        vehiculo.Marca = updateDto.Marca;
        vehiculo.Modelo = updateDto.Modelo;
        vehiculo.Año = updateDto.Año;
        vehiculo.KilometrajeActual = updateDto.KilometrajeActual;
        vehiculo.IntervaloServicioKm = updateDto.IntervaloServicioKm;
        // Recalcular el siguiente servicio
        vehiculo.SiguienteServicioKm = updateDto.KilometrajeActual + updateDto.IntervaloServicioKm;

        _context.Vehiculos.Update(vehiculo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> DeleteVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return NotFound();
        }

        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}