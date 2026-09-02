using BravoBack.Data;
using BravoBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente")]
    public class CategoriasVehiculosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasVehiculosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaVehiculo>>> GetCategorias()
        {
            return await _context.CategoriasVehiculo.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaVehiculo>> Create(CategoriaVehiculo categoria)
        {
            _context.CategoriasVehiculo.Add(categoria);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategorias), new { id = categoria.Id }, categoria);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.CategoriasVehiculo.FindAsync(id);
            if (cat == null) return NotFound();
            
            _context.CategoriasVehiculo.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
