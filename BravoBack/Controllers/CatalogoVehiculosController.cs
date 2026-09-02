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
    public class CatalogoVehiculosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogoVehiculosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // Para permitir que VehiculosController u otros consuman sin ser Gerente si fuese necesario, o lo dejo publico.
        public async Task<ActionResult<IEnumerable<CatalogoVehiculo>>> GetCatalogo()
        {
            return await _context.CatalogoVehiculos.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CatalogoVehiculo>> Create(CatalogoVehiculo catalogo)
        {
            _context.CatalogoVehiculos.Add(catalogo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCatalogo), new { id = catalogo.Id }, catalogo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CatalogoVehiculo catalogo)
        {
            if (id != catalogo.Id) return BadRequest();

            _context.Entry(catalogo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.CatalogoVehiculos.AnyAsync(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.CatalogoVehiculos.FindAsync(id);
            if (cat == null) return NotFound();
            
            _context.CatalogoVehiculos.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
