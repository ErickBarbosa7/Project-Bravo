namespace BravoBack.Services;

public interface IFileService
{
    // Recibe el archivo y una subcarpeta (ej. "vehiculos" o "incidentes")
    Task<string> GuardarImagenAsync(IFormFile imagen, string subcarpeta);
}