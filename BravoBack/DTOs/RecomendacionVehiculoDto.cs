namespace BravoBack.DTOs
{
    // DTO para enviar recomendaciones de un vehiculo
    // Por ejemplo si un vehiculo le faltan pocos km para el servicio 
    //y va a hacer una ruta larga, le recomienda uno que tenga el menos proximo 
    public class RecomendacionVehiculoDto
    {
        // Id del vehiculo en la base de datos
        public int VehiculoId { get; set; }

        // Placa del vehiculo
        public string Placa { get; set; } = string.Empty;

        // Modelo del vehiculo
        public string Modelo { get; set; } = string.Empty;

        // URL de la foto del vehiculo
        public string FotoUrl { get; set; } = string.Empty;
        
        // Datos relacionados con la recomendacion
        // Promedio de kilometros por litro que consume el vehiculo
        public double RendimientoKmPorLitro { get; set; }

        // Estimacion de litros que se gastaran en el viaje
        public double LitrosEstimadosParaViaje { get; set; }

        // Kilometros que faltan para el siguiente servicio
        public int KmRestantesParaServicio { get; set; }
    }
}
