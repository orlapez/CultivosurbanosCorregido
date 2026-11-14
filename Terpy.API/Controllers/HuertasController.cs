using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.API.Helpers;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;

namespace Terpy.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/huertas")]
    public class HuertasController : ControllerBase
    {
        private readonly DataContext _context;

        public HuertasController(DataContext context)
        {
            _context = context;
        }

        //listados y búsqueda por Id limitados al propietario

        private IQueryable<Huerta> HuertasAccesibles()
        {
            var query = _context.Huertas.AsQueryable();
            //En caso de ser diferente la referencia de matias, cambiar
            if (!User.EsAdministrador())
            {
                //En caso de ser diferente la referencia de matias, cambiar
                var usuarioId = User.ObtenerUsuarioId();
                query = query.Where(h => h.ResponsableId == usuarioId);
            }
            return query;
        }

        [HttpGet]
        public async Task<ActionResult<List<Huerta>>> Get()
        {
            var huertas = await HuertasAccesibles()
                .AsNoTracking()
                .OrderBy(h => h.Nombre)
                .ToListAsync();

            return Ok(huertas); // 200 si no hay registros
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Huerta>> Get(int id)
        {
            var huerta = await HuertasAccesibles()
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);

            if (huerta is null) return NotFound();
            return Ok(huerta);
        }

        [HttpPost]
        public async Task<ActionResult<Huerta>> Post(CrearHuertaDto dto)
        {
            //En caso de ser diferente la referencia de matias, cambiar
            var usuarioId = User.ObtenerUsuarioId();
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var nombre = dto.Nombre.Trim();
            var ubicacion = dto.Ubicacion.Trim();
            if (nombre.Length == 0)
                ModelState.AddModelError(nameof(dto.Nombre), "El nombre no puede estar en blanco.");
            if (ubicacion.Length == 0)
                ModelState.AddModelError(nameof(dto.Ubicacion), "La ubicación no puede estar en blanco.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState); // 400

            var huerta = new Huerta
            {
                //se asigna la responsabilidad desde la identidad autenticada
                Nombre = nombre,
                Ubicacion = ubicacion,
                ResponsableId = usuarioId
            };

            _context.Huertas.Add(huerta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = huerta.Id }, huerta);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, ActualizarHuertaDto dto)
        {
            var nombre = dto.Nombre.Trim();
            var ubicacion = dto.Ubicacion.Trim();
            if (nombre.Length == 0)
                ModelState.AddModelError(nameof(dto.Nombre), "El nombre no puede estar en blanco.");
            if (ubicacion.Length == 0)
                ModelState.AddModelError(nameof(dto.Ubicacion), "La ubicación no puede estar en blanco.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState); // 400

            var huerta = await HuertasAccesibles().FirstOrDefaultAsync(h => h.Id == id);
            if (huerta is null) return NotFound();

            // solo Nombre y ubicacion, el responsable no cambia.
            huerta.Nombre = nombre;
            huerta.Ubicacion = ubicacion;
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var huerta = await HuertasAccesibles().FirstOrDefaultAsync(h => h.Id == id);
            if (huerta is null) return NotFound();

            if (await _context.Zonas.AnyAsync(z => z.HuertaId == id))
                return Problem(
                    detail: "No se puede eliminar la huerta porque tiene zonas. Primero deben resolverse (eliminarse) las zonas.",
                    statusCode: StatusCodes.Status409Conflict);

            _context.Huertas.Remove(huerta);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}