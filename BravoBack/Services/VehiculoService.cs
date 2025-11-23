using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using BravoBack.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Services
{
    public class VehiculoService
    {
        private readonly AppDbContext _context;

        public VehiculoService(AppDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER TODOS
        public async Task<List<VehiculoDto>> ObtenerTodos()
        {
            return await _context.Vehiculos
                .Select(v => new VehiculoDto
                {
                    Id = v.Id,
                    Placa = v.Placa,
                    Nombre = v.Nombre,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Anio = v.Anio,
                    FotoUrl = v.FotoUrl,
                    KilometrajeActual = v.KilometrajeActual,
                    Estado = v.Estado.ToString(),
                    IntervaloServicioKm = v.IntervaloServicioKm,
                    SiguienteServicioKm = v.SiguienteServicioKm
                })
                .ToListAsync();
        }
    
        // 2. OBTENER POR ID
        public async Task<VehiculoDto?> ObtenerPorId(int id)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return null;
            return new VehiculoDto
            {
                Id = v.Id,
                Placa = v.Placa,
                Nombre = v.Nombre,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Anio = v.Anio,
                FotoUrl = v.FotoUrl,
                KilometrajeActual = v.KilometrajeActual,
                Estado = v.Estado.ToString(),
                IntervaloServicioKm = v.IntervaloServicioKm,
                SiguienteServicioKm = v.SiguienteServicioKm
            };
        }

        // 3. CREAR VEHÍCULO
        public async Task<VehiculoDto> CrearVehiculo(CreateVehiculoDto dto)
        {
            var vehiculo = new Vehiculo
            {
                Placa = dto.Placa,
                Nombre = dto.Nombre,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Anio = dto.Anio,
                FotoUrl = dto.FotoUrl,
                KilometrajeActual = dto.KilometrajeActual,
                IntervaloServicioKm = dto.IntervaloServicioKm,
                // Lógica de negocio: Calcular siguiente servicio
                SiguienteServicioKm = dto.KilometrajeActual + dto.IntervaloServicioKm,
                Estado = EstadoVehiculo.Disponible
            };

            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            return await ObtenerPorId(vehiculo.Id)!; // Reutilizamos el método para devolver el DTO completo
        }

        // 4. ACTUALIZAR VEHÍCULO
        public async Task<VehiculoDto?> ActualizarVehiculo(int id, UpdateVehiculoDto dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return null;

            // Actualizar campos
            vehiculo.Placa = dto.Placa;
            vehiculo.Nombre = dto.Nombre;
            vehiculo.Marca = dto.Marca;
            vehiculo.Modelo = dto.Modelo;
            vehiculo.Anio = dto.Anio;
            vehiculo.FotoUrl = dto.FotoUrl;
            vehiculo.KilometrajeActual = dto.KilometrajeActual;
            vehiculo.IntervaloServicioKm = dto.IntervaloServicioKm;
            
            vehiculo.SiguienteServicioKm = dto.KilometrajeActual + dto.IntervaloServicioKm;

            _context.Vehiculos.Update(vehiculo);
            await _context.SaveChangesAsync();

            return await ObtenerPorId(vehiculo.Id);
        }

        // 5. ELIMINAR VEHÍCULO
        public async Task<bool> EliminarVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return false;

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return true;
        }

        // 6. Obtener Estado Servicio
        public async Task<ReporteMantenimientoDto> ObtenerEstadoServicio(int vehiculoId)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
            
            // Si no existe, devolvemos un estado nulo o lanzamos excepción. 
            // Para este ejemplo, manejamos un default.
            if (vehiculo == null) 
            {
                return new ReporteMantenimientoDto { Mensaje = "Vehículo no encontrado", Estatus = EstatusMantenimiento.Desconocido };
            }

            int kmRestantes = vehiculo.SiguienteServicioKm - vehiculo.KilometrajeActual;

            // Objeto base
            var reporte = new ReporteMantenimientoDto
            {
                KmRestantes = kmRestantes
            };

            // Lógica del Semáforo 
            if (kmRestantes <= 0)
            {
                reporte.Estatus = EstatusMantenimiento.Vencido;
                reporte.Color = "ROJO";
                reporte.Mensaje = $"URGENTE: El servicio venció hace {Math.Abs(kmRestantes)} km.";
            }
            else if (kmRestantes <= 1000)
            {
                reporte.Estatus = EstatusMantenimiento.Preventivo;
                reporte.Color = "AMARILLO";
                reporte.Mensaje = $"Atención: Servicio próximo en {kmRestantes} km.";
            }
            else
            {
                reporte.Estatus = EstatusMantenimiento.Optimo;
                reporte.Color = "VERDE";
                reporte.Mensaje = $"Estado óptimo. Faltan {kmRestantes} km para servicio.";
            }

            return reporte;
        }

        // 7. Simular Pago
        public async Task<string> SimularPagoServicio(PagoServicioDTO pagoDto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(pagoDto.VehiculoId);
            if (vehiculo == null) return "Error: Vehículo no encontrado.";

            var nuevoRegistro = new RegistroServicio
            {
                VehiculoId = vehiculo.Id,
                MontoPagado = pagoDto.Monto,
                Estado = EstadoServicio.Pagado,
                KilometrajeServicio = vehiculo.KilometrajeActual,
                Fecha = DateTime.UtcNow
            };

            _context.RegistrosServicio.Add(nuevoRegistro);
            
            // Reiniciar contador
            vehiculo.SiguienteServicioKm = vehiculo.KilometrajeActual + vehiculo.IntervaloServicioKm;
            await _context.SaveChangesAsync();

            return "Pago procesado correctamente.";
        }

        // PREDICCIÓN DE PRESUPUESTO 
        public async Task<ProyeccionGastosDto> CalcularProyeccionMensual()
        {
            // 1. Calcular el Costo Histórico por Kilómetro
            // Sumamos todo el dinero gastado en servicios en la historia
            decimal totalGastado = await _context.RegistrosServicio.SumAsync(r => r.MontoPagado);
            
            // Sumamos todos los Km recorridos en la historia (según bitácoras)
            int totalKmRecorridosHistorico = await _context.BitacorasUso.SumAsync(b => b.KilometrosRecorridos);

            // Validación para evitar división entre cero (si el sistema es nuevo)
            if (totalKmRecorridosHistorico == 0)
            {
                return new ProyeccionGastosDto 
                { 
                    Mensaje = "No hay suficientes datos históricos de kilometraje para calcular proyecciones." 
                };
            }

            // Fórmula: ¿Cuánto nos cuesta cada Km que avanza la flota?
            // Convertimos a decimal para precisión monetaria
            decimal costoPorKm = totalGastado / (decimal)totalKmRecorridosHistorico;


            // 2. Calcular la Actividad Reciente (Últimos 30 días)
            DateTime haceUnMes = DateTime.UtcNow.AddDays(-30);
            
            int kmUltimoMes = await _context.BitacorasUso
                .Where(b => b.FechaUso >= haceUnMes)
                .SumAsync(b => b.KilometrosRecorridos);


            // 3. Proyectar el Futuro
            decimal proyeccion = costoPorKm * kmUltimoMes;

            return new ProyeccionGastosDto
            {
                CostoPromedioPorKm = Math.Round(costoPorKm, 2), // Ej: $0.50
                KmRecorridosUltimoMes = kmUltimoMes,            // Ej: 2000 km
                PresupuestoSugerido = Math.Round(proyeccion, 2), // Ej: $1000.00
                Mensaje = $"Basado en que la flota recorrió {kmUltimoMes} km el último mes y el costo promedio es de ${costoPorKm:F2}/km, se recomienda reservar ${proyeccion:F2} para mantenimiento."
            };
        }
       
    }
}