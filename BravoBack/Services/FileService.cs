using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BravoBack.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> GuardarImagenAsync(IFormFile imagen, string subcarpeta)
    {
        // 1. Validar que tengamos un wwwroot
        if (string.IsNullOrWhiteSpace(_env.WebRootPath))
        {
            _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        // 2. Definir la ruta de guardado
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", subcarpeta);
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        // 3. Crear un nombre único para el archivo
        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);

        // 4. Guardar el archivo en el disco
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await imagen.CopyToAsync(stream);
        }

        // 5. Devolver la URL pública que usará el frontend
        return $"/uploads/{subcarpeta}/{nombreArchivo}";
    }
}