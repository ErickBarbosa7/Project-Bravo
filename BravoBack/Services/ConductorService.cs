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

        // ==========================================
        // LISTAR TODOS LOS CONDUCTORES 
        // ==========================================
        public async Task<List<ConductorDto>> ObtenerListaConductores()
        {
            // Hacemos un Join entre Usuarios, Roles y la tabla intermedia
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
        // CALCULAR RENDIMIENTO DE COMBUSTIBLE
        public async Task<object> CalcularPorcentajeCombustible(string conductorId)
        {
            // Validar que el conductor exista
            var conductor = await _context.Users.FindAsync(conductorId);
            if (conductor == null)
            {
                return new { Error = "Conductor no encontrado" };
            }

            // Obtener el total de litros consumidos por TODA la empresa
            double totalEmpresa = await _context.BitacorasUso.SumAsync(b => b.LitrosConsumidos);

            if (totalEmpresa == 0)
            {
                return new 
                { 
                    Mensaje = "No hay registros de consumo de combustible en la empresa aún.",
                    TotalEmpresa = 0 
                };
            }

            // 3. Obtener el total de litros de ESTE conductor
            double totalConductor = await _context.BitacorasUso
                .Where(b => b.ConductorId == conductorId)
                .SumAsync(b => b.LitrosConsumidos);

            // 4. Calcular porcentaje
            double porcentaje = (totalConductor / totalEmpresa) * 100;

            // 5. Retornar un objeto con los datos detallados
            return new
            {
                Conductor = $"{conductor.FirstName} {conductor.PaternalLastName}",
                LitrosConsumidos = totalConductor,
                TotalEmpresa = totalEmpresa,
                PorcentajeDelTotal = Math.Round(porcentaje, 2), // Redondeado a 2 decimales
                Mensaje = $"El conductor ha consumido el {porcentaje:F2}% del combustible total de la flotilla."
            };
        }
        // ==========================================
        // REPORTE GENERAL DE FLOTILLA (Todos los conductores)
        // ==========================================
        public async Task<object> ObtenerReporteGeneral()
        {
            // 1. Obtener todos los registros de uso con datos del conductor
            var registros = await _context.BitacorasUso
                .Include(b => b.Conductor)
                .ToListAsync();

            // 2. Calcular el consumo TOTAL de la empresa
            double totalEmpresa = registros.Sum(r => r.LitrosConsumidos);

            if (totalEmpresa == 0) return new { Mensaje = "Aún no hay consumo registrado en la empresa." };

            // 3. Agrupar por Conductor
            var reporte = registros
                .GroupBy(r => r.Conductor)
                .Select(grupo => new
                {
                    ConductorId = grupo.Key.Id,
                    Nombre = $"{grupo.Key.FirstName} {grupo.Key.PaternalLastName}",
                    TotalLitros = grupo.Sum(r => r.LitrosConsumidos),
                    TotalKm = grupo.Sum(r => r.KilometrosRecorridos),
                    // Porcentaje respecto al total global
                    PorcentajeDelTotal = Math.Round((grupo.Sum(r => r.LitrosConsumidos) / totalEmpresa) * 100, 2)
                })
                .OrderByDescending(x => x.TotalLitros) // Los que más gastan arriba
                .ToList();

            return new
            {
                TotalEmpresaLitros = totalEmpresa,
                TotalConductores = reporte.Count,
                Desglose = reporte
            };
        }
        // ==========================================
        // REGISTRAR USO DIARIO (Conductor)
        // ==========================================
        public async Task<string> RegistrarUsoVehiculo(RegistrarUsoDto dto, string conductorId)
        {
            // 1. Buscar el vehículo
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
            if (vehiculo == null) return "Error: El vehículo no existe.";

            // 2. Crear el registro en BitacoraUso
            var nuevaBitacora = new BitacoraUso
            {
                VehiculoId = dto.VehiculoId,
                ConductorId = conductorId, // El ID viene del Token (seguro)
                KilometrosRecorridos = dto.KilometrosRecorridos,
                LitrosConsumidos = dto.LitrosConsumidos,
                FechaUso = DateTime.UtcNow
            };

            _context.BitacorasUso.Add(nuevaBitacora);

            // 3. LÓGICA AUTOMÁTICA: Actualizar el odómetro del vehículo
            // Al sumar esto, el sistema de "Semáforo" detectará si ya le toca servicio
            vehiculo.KilometrajeActual += dto.KilometrosRecorridos;

            // 4. Guardar todo
            await _context.SaveChangesAsync();

            return $"Registro exitoso. El kilometraje del auto {vehiculo.Placa} subió a {vehiculo.KilometrajeActual} Km.";
        }
    }
}