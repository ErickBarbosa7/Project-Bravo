using BravoBack.Data;
using BravoBack.Models;
using BravoBack.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
            // Se obtienen registros con datos del conductor
            var registros = await _context.BitacorasUso
                .Include(b => b.Conductor)
                .ToListAsync();

            // Consumo total de la empresa
            double totalEmpresa = registros.Sum(r => r.LitrosConsumidos);

            // Si no hay consumo registrado
            if (totalEmpresa == 0) 
                return new { Mensaje = "Aun no hay consumo registrado." };

            // Agrupar consumo por conductor
            var reporte = registros
                .GroupBy(r => r.Conductor)
                .Select(grupo => new
                {
                    ConductorId = grupo.Key.Id,
                    Nombre = $"{grupo.Key.FirstName} {grupo.Key.PaternalLastName}",
                    TotalLitros = grupo.Sum(r => r.LitrosConsumidos),
                    TotalKm = grupo.Sum(r => r.KilometrosRecorridos),
                    PorcentajeDelTotal = Math.Round((grupo.Sum(r => r.LitrosConsumidos) / totalEmpresa) * 100, 2)
                })
                .OrderByDescending(x => x.TotalLitros)
                .ToList();

            return new
            {
                TotalEmpresaLitros = totalEmpresa,
                TotalConductores = reporte.Count,
                Desglose = reporte
            };
        }

        // Registra el uso diario de un vehiculo por un conductor
        public async Task<string> RegistrarUsoVehiculo(RegistrarUsoDto dto, string conductorId)
        {
            // Se valida que el vehiculo exista
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
            if (vehiculo == null) return "Error: El vehiculo no existe.";

            // Crear registro nuevo en la bitacora
            var nuevaBitacora = new BitacoraUso
            {
                VehiculoId = dto.VehiculoId,
                ConductorId = conductorId,
                KilometrosRecorridos = dto.KilometrosRecorridos,
                LitrosConsumidos = dto.LitrosConsumidos,
                FechaUso = DateTime.UtcNow
            };

            _context.BitacorasUso.Add(nuevaBitacora);

            // Se actualiza el kilometraje del vehiculo
            vehiculo.KilometrajeActual += dto.KilometrosRecorridos;

            // Se guardan los cambios
            await _context.SaveChangesAsync();

            return $"Registro exitoso. El kilometraje del auto {vehiculo.Placa} ahora es {vehiculo.KilometrajeActual} km.";
        }
    }
}
