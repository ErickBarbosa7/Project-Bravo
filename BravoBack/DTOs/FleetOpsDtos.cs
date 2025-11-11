using System.ComponentModel.DataAnnotations;

namespace BravoBack.DTOs;

// DTO para registrar una salida
public class CheckOutDto
{
    [Required]
    public int VehiculoId { get; set; }
    public string? Destino { get; set; }
}

// DTO para registrar una entrada
public class CheckInDto
{
    [Required]
    public int BitacoraViajeId { get; set; } // El ID del viaje que estás cerrando

    [Required]
    [Range(1, 10000000)] 
    public int KilometrajeFinal { get; set; }
}

// DTO para registrar un servicio completado
public class ServiceCompletedDto
{
    [Required]
    public int VehiculoId { get; set; }
    
    [Required]
    public string DescripcionServicio { get; set; } = string.Empty;
    
    public decimal? Costo { get; set; }
}