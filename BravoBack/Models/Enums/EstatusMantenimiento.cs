namespace BravoBack.Models.Enums
{
    public enum EstatusMantenimiento
    {
        Optimo = 0,      // Verde
        Preventivo = 1,  // Amarillo (Próximo a servicio)
        Vencido = 2,     // Rojo (Urgente)
        Desconocido = 3  
    }
}