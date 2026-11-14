using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;
using Terpy.API.Helpers;

namespace Terpy.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/zonas")]
    public class ZonasController : ControllerBase
    {
        private readonly DataContext _context;

        public ZonasController(DataContext context)
        {
            _context = context;
        }

        // Zona no tiene propiedad de navegación a Huerta, así que se filtra con una subconsulta.
        private IQueryable<Zona> ZonasAccesibles()
        {
            var query = _context.Zonas.AsQueryable();
            if (!User.EsAdministrador())
            {
                var usuarioId = User.ObtenerUsuarioId();
                query = query.Where(z => _context.Huertas
                    .Any(h => h.Id == z.HuertaId && h.ResponsableId == usuarioId));
            }
            return query;
        }

        //Revisa si el usuario puede acceder a la huerta
        private async Task<bool> HuertaAccesibleAsync(int huertaId)
        {
            var query = _context.Huertas.Where(h => h.Id == huertaId);
            //En caso de ser diferente la referencia de matias, cambiar
            if (!User.EsAdministrador())
            {
                //En caso de ser diferente la referencia de matias, cambiar
                var usuarioId = User.ObtenerUsuarioId();
                query = query.Where(h => h.ResponsableId == usuarioId);
            }
            return await query.AnyAsync();
        }

        [HttpGet]
        public async Task<ActionResult<List<Zona>>> Get([FromQuery] int? huertaId)
        {
            var query = ZonasAccesibles().AsNoTracking();
            if (huertaId.HasValue)
                query = query.Where(z => z.HuertaId == huertaId.Value);

            var zonas = await query
                .OrderBy(z => z.HuertaId)
                .ThenBy(z => z.Nombre)
                .ToListAsync();

            return Ok(zonas); // 200
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Zona>> Get(int id)
        {
            var zona = await ZonasAccesibles()
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zona is null) return NotFound(); 
            return Ok(zona);
        }

        [HttpPost]
        public async Task<ActionResult<Zona>> Post(CrearZonaDto dto)
        {
            var nombre = dto.Nombre.Trim();
            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            if (nombre.Length == 0)
                ModelState.AddModelError(nameof(dto.Nombre), "El nombre no puede estar en blanco.");
            if (!await HuertaAccesibleAsync(dto.HuertaId))
                ModelState.AddModelError(nameof(dto.HuertaId), "La huerta indicada no existe o no es accesible.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState); // 400

            // El nombre no se puede repetir en la misma huerta
            if (await _context.Zonas.AnyAsync(z => z.HuertaId == dto.HuertaId && z.Nombre == nombre))
                return Problem(detail: "Ya existe una zona con ese nombre en esta huerta.",
                    statusCode: StatusCodes.Status409Conflict);

            var zona = new Zona
            {
                HuertaId = dto.HuertaId,
                Nombre = nombre,
                Descripcion = descripcion
            };

            _context.Zonas.Add(zona);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Problem(detail: "No se pudo crear la zona: el nombre ya está en uso en esta huerta.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            return CreatedAtAction(nameof(Get), new { id = zona.Id }, zona); // 201 + zona
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, ActualizarZonaDto dto)
        {
            var nombre = dto.Nombre.Trim();
            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            if (nombre.Length == 0)
                ModelState.AddModelError(nameof(dto.Nombre), "El nombre no puede estar en blanco.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState); // 400

            var zona = await ZonasAccesibles().FirstOrDefaultAsync(z => z.Id == id);
            if (zona is null) return NotFound();

            if (await _context.Zonas.AnyAsync(z =>
                    z.HuertaId == zona.HuertaId && z.Id != id && z.Nombre == nombre))
                return Problem(detail: "Ya existe otra zona con ese nombre en esta huerta.",
                    statusCode: StatusCodes.Status409Conflict);

            zona.Nombre = nombre;
            zona.Descripcion = descripcion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Problem(detail: "No se pudo actualizar la zona: el nombre ya está en uso en esta huerta.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            return NoContent(); // 204
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var zona = await ZonasAccesibles().FirstOrDefaultAsync(z => z.Id == id);
            if (zona is null) return NotFound();

            if (await _context.Siembras.AnyAsync(s => s.ZonaId == id))
                return Problem(
                    detail: "No se puede eliminar la zona porque tiene siembras. Primero deben resolverse las siembras.",
                    statusCode: StatusCodes.Status409Conflict);

            _context.Zonas.Remove(zona);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}

