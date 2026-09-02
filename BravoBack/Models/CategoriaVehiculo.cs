using System.ComponentModel.DataAnnotations;

namespace BravoBack.Models;

public class CategoriaVehiculo
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nombre { get; set; } = string.Empty;
}
