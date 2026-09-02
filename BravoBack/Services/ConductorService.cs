using BravoBack.Data;
using BravoBack.Models;
using BravoBack.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BravoBack.Models.Enums;

namespace BravoBack.Services
{
    public class ConductorService
    {
        private readonly AppDbContext _context;

        public ConductorService(AppDbContext context)
        {
            _context = context;
        }

        // Lista todos los conductores registrados con rol "Conductor"
        public async Task<List<ConductorDto>> ObtenerListaConductores()
        {
            // Se hace join entre usuarios, roles y relacion intermedia
            var query = from user in _context.Users
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == "Conductor"
                        select new ConductorDto
                        {
                            Id = user.Id,
                            NombreCompleto = $"{user.FirstName} {user.PaternalLastName}",
                            Email = user.Email!
                        };

            return await query.ToListAsync();
        }

        // Calcula el porcentaje de combustible que gasto un conductor respecto al total de la empresa
        public async Task<object> CalcularPorcentajeCombustible(string conductorId)
        {
            // Primero validamos que el conductor exista
            var conductor = await _context.Users.FindAsync(conductorId);
            if (conductor == null)
            {
                return new { Error = "Conductor no encontrado" };
            }

            // Litros consumidos por toda la empresa
            double totalEmpresa = await _context.BitacorasUso.SumAsync(b => b.LitrosConsumidos);

            // Si no hay registros se devuelve un mensaje
            if (totalEmpresa == 0)
            {
                return new
                {
                    Mensaje = "No hay consumo registrado en la empresa aun",
                    TotalEmpresa = 0
                };
            }

            // Litros consumidos por el conductor especifico
            double totalConductor = await _context.BitacorasUso
                .Where(b => b.ConductorId == conductorId)
                .SumAsync(b => b.LitrosConsumidos);

            // Porcentaje calculado respecto al total
            double porcentaje = (totalConductor / totalEmpresa) * 100;

            return new
            {
                Conductor = $"{conductor.FirstName} {conductor.PaternalLastName}",
                LitrosConsumidos = totalConductor,
                TotalEmpresa = totalEmpresa,
                PorcentajeDelTotal = Math.Round(porcentaje, 2),
                Mensaje = $"El conductor ha consumido el {porcentaje:F2}% del combustible total."
            };
        }

        // Reporte general de consumo por conductor para toda la flota
        public async Task<object> ObtenerReporteGeneral()
        {
            // Consumo total de la empresa (calculado en SQL)
            double totalEmpresa = await _context.BitacorasUso.SumAsync(r => r.LitrosConsumidos);

            if (totalEmpresa == 0) 
                return new { Mensaje = "Aun no hay consumo registrado." };

            // Agrupación y suma directamente en SQL para evitar cargar toda la tabla en memoria
            var reporte = await _context.BitacorasUso
                .GroupBy(b => new { b.ConductorId, b.Conductor.FirstName, b.Conductor.PaternalLastName })
                .Select(g => new
                {
                    ConductorId = g.Key.ConductorId,
                    Nombre = $"{g.Key.FirstName} {g.Key.PaternalLastName}",
                    TotalLitros = g.Sum(r => r.LitrosConsumidos),
                    TotalKm = g.Sum(r => r.KilometrosRecorridos)
                })
                .OrderByDescending(x => x.TotalLitros)
                .ToListAsync();

            // Cálculo de porcentajes
            var desglose = reporte.Select(x => new 
            {
                x.ConductorId,
                x.Nombre,
                x.TotalLitros,
                x.TotalKm,
                PorcentajeDelTotal = Math.Round((x.TotalLitros / totalEmpresa) * 100, 2)
            }).ToList();

            return new
            {
                TotalEmpresaLitros = totalEmpresa,
                TotalConductores = desglose.Count,
                Desglose = desglose
            };
        }

        // Registra el uso diario de un vehiculo por un conductor
        public async Task<string> RegistrarUsoVehiculo(RegistrarUsoDto dto, string conductorId)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
            if (vehiculo == null) return "Error: El vehículo no existe.";

            if (vehiculo.Estado == EstadoVehiculo.EnTaller) 
                return "Error: No se puede registrar uso de un vehículo que está en el taller.";

            // A. Crear Bitácora
            var nuevaBitacora = new BitacoraUso
            {
                VehiculoId = dto.VehiculoId,
                ConductorId = conductorId,
                KilometrosRecorridos = dto.KilometrosRecorridos,
                LitrosConsumidos = dto.LitrosConsumidos,
                FechaUso = DateTime.UtcNow
            };

            _context.BitacorasUso.Add(nuevaBitacora);

            // B. Actualizar Odómetro
            vehiculo.KilometrajeActual += dto.KilometrosRecorridos;

            // Si se pasó del kilometraje meta, cambiamos el estado a ROJO (Necesita Servicio)
            if (vehiculo.KilometrajeActual >= vehiculo.SiguienteServicioKm)
            {
                vehiculo.Estado = EstadoVehiculo.NecesitaServicio; // Rojo (3)
            }

            await _context.SaveChangesAsync();

            return $"Registro exitoso. El kilometraje del auto {vehiculo.Placa} subió a {vehiculo.KilometrajeActual} Km.";
        }
    }
}
