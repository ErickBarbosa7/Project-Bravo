namespace BravoBack.DTOs;

public class CheckOutDto
{
    public int VehiculoId { get; set; }
    public string? Destino { get; set; }
}

public class CheckInDto
{
    public int BitacoraViajeId { get; set; }
    public int KilometrajeFinal { get; set; }
}

public class ServiceCompletedDto
{
    public int VehiculoId { get; set; }
    public string DescripcionServicio { get; set; } = string.Empty;
    public decimal? Costo { get; set; }
}