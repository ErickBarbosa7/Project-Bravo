using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation; 

namespace BravoBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class VehiculosController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiculosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/vehiculos 
    [HttpGet]
    [Authorize(Roles = "Gerente,Conductor")]
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
                Estado = v.Estado.ToString(),
                IntervaloServicioKm = v.IntervaloServicioKm,
                SiguienteServicioKm = v.SiguienteServicioKm
            })
            .ToListAsync();

        return Ok(vehiculos);
    }

    // GET: api/vehiculos/5 
    [HttpGet("{id}")]
    [Authorize(Roles = "Gerente,Conductor")]
    public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
    {
        var v = await _context.Vehiculos.FindAsync(id);
        if (v == null) return NotFound();
       
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

    [HttpPost]
[Authorize(Roles = "Gerente")]
public async Task<ActionResult<VehiculoDto>> CreateVehiculo(
    [FromBody] CreateVehiculoDto createDto,
    [FromServices] IValidator<CreateVehiculoDto> validator)
{
    var validationResult = await validator.ValidateAsync(createDto);
    if (!validationResult.IsValid)
    {
        return BadRequest(validationResult.ToDictionary());
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
        SiguienteServicioKm = createDto.KilometrajeActual + createDto.IntervaloServicioKm,
        Estado = EstadoVehiculo.Disponible
    };

    _context.Vehiculos.Add(vehiculo);
    await _context.SaveChangesAsync();

    // En lugar de castear, creamos el DTO
    var vehiculoDto = new VehiculoDto
    {
        Id = vehiculo.Id,
        Placa = vehiculo.Placa,
        Nombre = vehiculo.Nombre,
        Marca = vehiculo.Marca,
        Modelo = vehiculo.Modelo,
        Año = vehiculo.Año,
        FotoUrl = vehiculo.FotoUrl,
        KilometrajeActual = vehiculo.KilometrajeActual,
        Estado = vehiculo.Estado.ToString(),
        IntervaloServicioKm = vehiculo.IntervaloServicioKm,
        SiguienteServicioKm = vehiculo.SiguienteServicioKm
    };

    return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.Id }, vehiculoDto);
}
    
    // PUT api/vehiculos/5 
    [HttpPut("{id}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> UpdateVehiculo(
        int id, 
        [FromBody] UpdateVehiculoDto updateDto,
        [FromServices] IValidator<UpdateVehiculoDto> validator) 
    {
        // Llenamos el ID en el DTO para que el validador pueda usarlo
        updateDto.Id = id;

        var validationResult = await validator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return NotFound(); 
        }

        // Mapear campos
        vehiculo.Placa = updateDto.Placa;
        vehiculo.Nombre = updateDto.Nombre;
        vehiculo.Marca = updateDto.Marca;
        vehiculo.Modelo = updateDto.Modelo;
        vehiculo.Año = updateDto.Año;
        vehiculo.KilometrajeActual = updateDto.KilometrajeActual;
        vehiculo.IntervaloServicioKm = updateDto.IntervaloServicioKm;
        vehiculo.SiguienteServicioKm = updateDto.KilometrajeActual + updateDto.IntervaloServicioKm;

        _context.Vehiculos.Update(vehiculo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/vehiculos/5 
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