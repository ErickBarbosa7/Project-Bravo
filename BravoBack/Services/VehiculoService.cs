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

        // Obtiene todos los vehiculos y los transforma en DTO
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
                    Estado = v.Estado,
                    IntervaloServicioKm = v.IntervaloServicioKm,
                    SiguienteServicioKm = v.SiguienteServicioKm
                })
                .ToListAsync();
        }

        // Busca un vehiculo por su id y lo convierte en DTO
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
                Estado = v.Estado,
                IntervaloServicioKm = v.IntervaloServicioKm,
                SiguienteServicioKm = v.SiguienteServicioKm
            };
        }

        // Crea un nuevo vehiculo en la base de datos
        public async Task<VehiculoDto> CrearVehiculo(CreateVehiculoDto dto)
        {
            // Se arma la entidad usando los datos del DTO
            var vehiculo = new Vehiculo
            {
                Placa = dto.Placa,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Nombre = $"{dto.Marca} {dto.Modelo}", // Se genera un nombre compuesto
                Anio = dto.Anio,
                FotoUrl = dto.FotoUrl,
                KilometrajeActual = dto.KilometrajeActual,
                IntervaloServicioKm = dto.IntervaloServicioKm,
                // Calcula cuando tocara el siguiente servicio
                SiguienteServicioKm = dto.KilometrajeActual + dto.IntervaloServicioKm,
                Estado = EstadoVehiculo.Disponible
            };

            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            return await ObtenerPorId(vehiculo.Id)!;
        }

        // Actualiza un vehiculo existente
        public async Task<VehiculoDto?> ActualizarVehiculo(int id, UpdateVehiculoDto dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return null;

            // Se actualizan los campos principales del vehiculo
            vehiculo.Placa = dto.Placa;
            vehiculo.Nombre = $"{dto.Marca} {dto.Modelo}";
            vehiculo.Marca = dto.Marca;
            vehiculo.Modelo = dto.Modelo;
            vehiculo.Anio = dto.Anio;
            vehiculo.FotoUrl = dto.FotoUrl;
            vehiculo.KilometrajeActual = dto.KilometrajeActual;
            vehiculo.IntervaloServicioKm = dto.IntervaloServicioKm;

            // Se recalcula cuando le toca el siguiente servicio
            vehiculo.SiguienteServicioKm = dto.KilometrajeActual + dto.IntervaloServicioKm;

            _context.Vehiculos.Update(vehiculo);
            await _context.SaveChangesAsync();

            return await ObtenerPorId(vehiculo.Id);
        }

        // Elimina un vehiculo de la base de datos
        public async Task<bool> EliminarVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return false;

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return true;
        }

        // Calcula el estado del servicio segun los kilometros restantes
        public async Task<ReporteMantenimientoDto> ObtenerEstadoServicio(int vehiculoId)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);

            if (vehiculo == null)
            {
                return new ReporteMantenimientoDto
                {
                    Mensaje = "Vehiculo no encontrado",
                    Estatus = EstatusMantenimiento.Desconocido,
                    EstadoVehiculo = EstadoVehiculo.Disponible // valor por defecto
                };
            }

            int kmRestantes = vehiculo.SiguienteServicioKm - vehiculo.KilometrajeActual;

            var reporte = new ReporteMantenimientoDto
            {
                KmRestantes = kmRestantes,
                EstadoVehiculo = vehiculo.Estado // 🔹 aquí mandas el estado real
            };

            if (kmRestantes <= 0)
            {
                reporte.Estatus = EstatusMantenimiento.Vencido;
                reporte.Color = "ROJO";
                reporte.Mensaje = $"El servicio ya venció hace {Math.Abs(kmRestantes)} km.";
            }
            else if (kmRestantes <= 1000)
            {
                reporte.Estatus = EstatusMantenimiento.Preventivo;
                reporte.Color = "AMARILLO";
                reporte.Mensaje = $"El servicio está próximo en {kmRestantes} km.";
            }
            else
            {
                reporte.Estatus = EstatusMantenimiento.Optimo;
                reporte.Color = "VERDE";
                reporte.Mensaje = $"Aún faltan {kmRestantes} km para el próximo servicio.";
            }

            return reporte;
        }


        // Guarda un pago de servicio y reinicia el contador
        public async Task<string> SimularPagoServicio(PagoServicioDTO pagoDto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(pagoDto.VehiculoId);
            if (vehiculo == null) return "Error: Vehiculo no encontrado.";

            // Se registra un nuevo servicio en el historial
            var nuevoRegistro = new RegistroServicio
            {
                VehiculoId = vehiculo.Id,
                MontoPagado = pagoDto.Monto,
                Estado = EstadoServicio.Pagado,
                KilometrajeServicio = vehiculo.KilometrajeActual,
                Fecha = DateTime.UtcNow
            };

            _context.RegistrosServicio.Add(nuevoRegistro);

            // Se reinicia el kilometraje para el proximo servicio
            vehiculo.SiguienteServicioKm = vehiculo.KilometrajeActual + vehiculo.IntervaloServicioKm;
            vehiculo.Estado = EstadoVehiculo.Disponible;
            await _context.SaveChangesAsync();

            return "Pago registrado correctamente.";
        }

        // Cambia el estado de un vehiculo para enviarlo al taller
        public async Task<bool> EnviarATaller(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return false;

            vehiculo.Estado = EstadoVehiculo.EnTaller;

            await _context.SaveChangesAsync();
            return true;
        }

        // Calcula una proyeccion mensual de mantenimiento
        public async Task<ProyeccionGastosDto> CalcularProyeccionMensual()
        {
            // Suma del dinero gastado en toda la historia de servicios
            decimal totalGastado = await _context.RegistrosServicio.SumAsync(r => r.MontoPagado);

            // Suma del total de kilometros recorridos historicos
            int totalKmRecorridos = await _context.BitacorasUso.SumAsync(b => b.KilometrosRecorridos);

            // Si no hay datos no se pueden generar proyecciones
            if (totalKmRecorridos == 0)
            {
                return new ProyeccionGastosDto
                {
                    Mensaje = "No hay datos suficientes para generar una proyeccion."
                };
            }

            // Costo por kilometro basado en la historia completa
            decimal costoPorKm = totalGastado / (decimal)totalKmRecorridos;

            // Se obtienen los kilometros del ultimo mes
            DateTime haceUnMes = DateTime.UtcNow.AddDays(-30);

            int kmUltimoMes = await _context.BitacorasUso
                .Where(b => b.FechaUso >= haceUnMes)
                .SumAsync(b => b.KilometrosRecorridos);

            // Multiplica lo recorrido por el costo promedio
            decimal proyeccion = costoPorKm * kmUltimoMes;

            return new ProyeccionGastosDto
            {
                CostoPromedioPorKm = Math.Round(costoPorKm, 2),
                KmRecorridosUltimoMes = kmUltimoMes,
                PresupuestoSugerido = Math.Round(proyeccion, 2),
                Mensaje = $"Se recomienda reservar {proyeccion:F2} basado en la actividad del ultimo mes."
            };
        }

        public async Task<List<RecomendacionVehiculoDto>> RecomendarVehiculos(int distanciaViaje)
        {
            // 1. Traemos los vehiculos disponibles
            // Incluimos las bitacoras para calcular eficiencia despues
            var candidatos = await _context.Vehiculos
                .Where(v => v.Estado == EstadoVehiculo.Disponible)
                .Include(v => v.BitacoraViajes) // O BitacorasUso segun como tengas la relacion
                .ToListAsync();

            var recomendaciones = new List<RecomendacionVehiculoDto>();

            foreach (var v in candidatos)
            {
                // Evitamos autos que puedan pasar el proximo servicio o queden muy justos
                int kmAlFinalizarViaje = v.KilometrajeActual + distanciaViaje;
                if (kmAlFinalizarViaje + 100 >= v.SiguienteServicioKm)
                {
                    continue; // Saltamos este auto, no es seguro para el viaje
                }

                // Traemos historial de consumo de este auto
                var logs = await _context.BitacorasUso
                    .Where(b => b.VehiculoId == v.Id)
                    .ToListAsync();

                double rendimientoPromedio = 0;

                if (logs.Any())
                {
                    double totalKm = logs.Sum(l => l.KilometrosRecorridos);
                    double totalLitros = logs.Sum(l => l.LitrosConsumidos);
                    // Evitamos division entre cero
                    rendimientoPromedio = totalLitros > 0 ? totalKm / totalLitros : 0;
                }
                else
                {
                    // Si no hay datos, usamos un promedio estimado (10 km/l)
                    rendimientoPromedio = 10;
                }

                // Agregamos el auto a la lista de aprobados
                recomendaciones.Add(new RecomendacionVehiculoDto
                {
                    VehiculoId = v.Id,
                    Placa = v.Placa,
                    Modelo = $"{v.Marca} {v.Modelo}",
                    FotoUrl = v.FotoUrl ?? "",
                    KmRestantesParaServicio = v.SiguienteServicioKm - v.KilometrajeActual,

                    // Datos calculados
                    RendimientoKmPorLitro = Math.Round(rendimientoPromedio, 2),
                    // Estimacion de litros para el viaje
                    LitrosEstimadosParaViaje = Math.Round(distanciaViaje / (rendimientoPromedio > 0 ? rendimientoPromedio : 10), 1)
                });
            }

            // Ordenamos los autos por mayor rendimiento primero
            return recomendaciones.OrderByDescending(r => r.RendimientoKmPorLitro).ToList();
        }

    }
}
