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
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return null;
            return MapToDto(vehiculo);
        }

        // Obtiene el catalogo de vehiculos predefinidos
        public async Task<List<CatalogoVehiculo>> ObtenerCatalogo()
        {
            return await _context.CatalogoVehiculos.ToListAsync();
        }

        private VehiculoDto MapToDto(Vehiculo v)
        {
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

            if (dto.GuardarEnCatalogo)
            {
                var existe = await _context.CatalogoVehiculos.AnyAsync(c => c.Marca == dto.Marca && c.Modelo == dto.Modelo && c.Anio == dto.Anio);
                if (!existe)
                {
                    _context.CatalogoVehiculos.Add(new CatalogoVehiculo
                    {
                        Marca = dto.Marca ?? "Desconocido",
                        Modelo = dto.Modelo ?? "Desconocido",
                        Anio = dto.Anio,
                        Categoria = "General",
                        IntervaloServicioKm = dto.IntervaloServicioKm,
                        FotoUrl = dto.FotoUrl ?? "assets/vehiculos/vehiculo-placeholder.png"
                    });
                    await _context.SaveChangesAsync();
                }
            }

            return MapToDto(vehiculo);
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

            // CORRECCIÓN DRIFT: Calcular sobre la meta original. 
            vehiculo.SiguienteServicioKm += vehiculo.IntervaloServicioKm;

            // Si el kilometraje actual aún supera el siguiente servicio (por ej. si omitió varios mantenimientos)
            if (vehiculo.SiguienteServicioKm <= vehiculo.KilometrajeActual)
            {
                vehiculo.SiguienteServicioKm = vehiculo.KilometrajeActual + vehiculo.IntervaloServicioKm;
            }
            
            // DESACOPLAMIENTO: El pago NO libera el auto automáticamente.
            // vehiculo.Estado = EstadoVehiculo.Disponible; 

            await _context.SaveChangesAsync();

            return "Pago registrado correctamente. El auto permanecerá en el taller hasta que sea liberado operativamente.";
        }

        // Libera un vehiculo del taller
        public async Task<bool> LiberarDeTaller(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return false;

            vehiculo.Estado = EstadoVehiculo.Disponible;

            await _context.SaveChangesAsync();
            return true;
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
            var vehiculos = await _context.Vehiculos.ToListAsync();
            
            DateTime haceUnMes = DateTime.UtcNow.AddDays(-30);
            
            // Consumo del ultimo mes por vehiculo
            var usoUltimoMes = await _context.BitacorasUso
                .Where(b => b.FechaUso >= haceUnMes)
                .GroupBy(b => b.VehiculoId)
                .Select(g => new {
                    VehiculoId = g.Key,
                    KmRecorridos = g.Sum(b => b.KilometrosRecorridos)
                }).ToDictionaryAsync(x => x.VehiculoId);

            // Costo promedio de servicio por vehiculo
            var costosServicio = await _context.RegistrosServicio
                .GroupBy(r => r.VehiculoId)
                .Select(g => new {
                    VehiculoId = g.Key,
                    CostoPromedio = g.Average(r => r.MontoPagado)
                }).ToDictionaryAsync(x => x.VehiculoId);

            decimal costoGlobalPromedio = await _context.RegistrosServicio.AnyAsync() 
                ? await _context.RegistrosServicio.AverageAsync(r => r.MontoPagado) 
                : 0m;

            decimal presupuestoSugerido = 0m;
            int totalKmUltimoMes = 0;
            int autosMantenimientoProximo = 0;

            foreach(var v in vehiculos)
            {
                int kmMes = usoUltimoMes.TryGetValue(v.Id, out var uso) ? uso.KmRecorridos : 0;
                totalKmUltimoMes += kmMes;

                // Si el vehiculo necesita servicio ahora o lo necesitara en los proximos 30 dias (asumiendo uso constante)
                if (v.KilometrajeActual + kmMes >= v.SiguienteServicioKm)
                {
                    decimal costoEstimado = costosServicio.TryGetValue(v.Id, out var costo) 
                        ? costo.CostoPromedio 
                        : costoGlobalPromedio;
                    
                    presupuestoSugerido += costoEstimado;
                    autosMantenimientoProximo++;
                }
            }

            // Calculo para estadistica general (CostoPorKm)
            decimal totalGastado = await _context.RegistrosServicio.SumAsync(r => r.MontoPagado);
            int totalKmRecorridos = await _context.BitacorasUso.SumAsync(b => b.KilometrosRecorridos);
            decimal costoPorKm = totalKmRecorridos > 0 ? totalGastado / (decimal)totalKmRecorridos : 0;

            return new ProyeccionGastosDto
            {
                CostoPromedioPorKm = Math.Round(costoPorKm, 2),
                KmRecorridosUltimoMes = totalKmUltimoMes,
                PresupuestoSugerido = Math.Round(presupuestoSugerido, 2),
                Mensaje = autosMantenimientoProximo > 0 
                    ? $"Se proyectan {autosMantenimientoProximo} auto(s) para mantenimiento el próximo mes." 
                    : "No se proyectan mantenimientos para el próximo mes con el uso actual."
            };
        }

        public async Task<List<RecomendacionVehiculoDto>> RecomendarVehiculos(int distanciaViaje)
        {
            // 1. Traemos los vehiculos disponibles que puedan completar el viaje con un margen de seguridad de 100km
            var vehiculosAprobados = await _context.Vehiculos
                .Where(v => v.Estado == EstadoVehiculo.Disponible && 
                           (v.KilometrajeActual + distanciaViaje + 100) < v.SiguienteServicioKm)
                .ToListAsync();

            if (!vehiculosAprobados.Any()) return new List<RecomendacionVehiculoDto>();

            var vehiculoIds = vehiculosAprobados.Select(v => v.Id).ToList();

            // 2. Traemos el historial de consumo agregado por vehiculo en una sola consulta
            var rendimientos = await _context.BitacorasUso
                .Where(b => vehiculoIds.Contains(b.VehiculoId))
                .GroupBy(b => b.VehiculoId)
                .Select(g => new
                {
                    VehiculoId = g.Key,
                    TotalKm = g.Sum(b => b.KilometrosRecorridos),
                    TotalLitros = g.Sum(b => b.LitrosConsumidos)
                })
                .ToDictionaryAsync(x => x.VehiculoId);

            var recomendaciones = new List<RecomendacionVehiculoDto>();

            foreach (var v in vehiculosAprobados)
            {
                double rendimientoPromedio = 10.0; // Valor por defecto si no hay datos

                if (rendimientos.TryGetValue(v.Id, out var stats) && stats.TotalLitros > 0)
                {
                    rendimientoPromedio = (double)stats.TotalKm / stats.TotalLitros;
                }

                recomendaciones.Add(new RecomendacionVehiculoDto
                {
                    VehiculoId = v.Id,
                    Placa = v.Placa,
                    Modelo = $"{v.Marca} {v.Modelo}",
                    FotoUrl = v.FotoUrl ?? "",
                    KmRestantesParaServicio = v.SiguienteServicioKm - v.KilometrajeActual,
                    RendimientoKmPorLitro = Math.Round(rendimientoPromedio, 2),
                    LitrosEstimadosParaViaje = Math.Round(distanciaViaje / rendimientoPromedio, 1)
                });
            }

            return recomendaciones.OrderByDescending(r => r.RendimientoKmPorLitro).ToList();
        }

    }
}
