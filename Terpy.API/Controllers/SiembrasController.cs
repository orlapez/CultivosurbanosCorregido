using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.API.Helpers;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;
using Terpy.Shared.Enums;

namespace Terpy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SiembrasController : ControllerBase
    {
        private readonly DataContext _context;

        public SiembrasController(DataContext context)
        {
            _context = context;
        }

        private IQueryable<Zona> ZonasAccesibles()
        {
            var zonas = _context.Zonas.AsQueryable();
            if (User.EsAdministrador())
            {
                return zonas;
            }

            var usuarioId = User.ObtenerUsuarioId();
            return zonas.Where(z => _context.Huertas.Any(h =>
                h.Id == z.HuertaId && h.ResponsableId == usuarioId));
        }

        private IQueryable<Siembra> SiembrasAccesibles()
        {
            var zonas = ZonasAccesibles();
            return _context.Siembras.Where(s => zonas.Any(z => z.Id == s.ZonaId));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Siembra>>> GetSiembras()
        {
            var siembras = await SiembrasAccesibles()
                .AsNoTracking()
                .OrderByDescending(s => s.FechaSiembra)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return Ok(siembras);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Siembra>> GetSiembra(int id)
        {
            var siembra = await SiembrasAccesibles()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (siembra == null)
            {
                return NotFound();
            }

            return Ok(siembra);
        }

        [HttpPost]
        public async Task<ActionResult<Siembra>> PostSiembra(CrearSiembraDto crearDto)
        {
            var errorFecha = ValidarFechaSiembra(crearDto.FechaSiembra);
            if (errorFecha is not null)
            {
                return BadRequest(errorFecha);
            }

            if (!await ZonasAccesibles().AnyAsync(z => z.Id == crearDto.ZonaId))
            {
                return NotFound();
            }

            if (!await _context.Cultivos.AnyAsync(c => c.Id == crearDto.CultivoId))
            {
                return NotFound("El cultivo indicado no existe.");
            }

            var siembra = new Siembra
            {
                ZonaId = crearDto.ZonaId,
                CultivoId = crearDto.CultivoId,
                FechaSiembra = crearDto.FechaSiembra.Date,
                CantidadPlantasInicial = crearDto.CantidadPlantasInicial,
                Estado = EstadoSiembra.Activa
            };

            _context.Siembras.Add(siembra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSiembra), new { id = siembra.Id }, siembra);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutSiembra(int id, ActualizarSiembraDto actualizarDto)
        {
            var siembra = await SiembrasAccesibles().FirstOrDefaultAsync(s => s.Id == id);
            if (siembra == null)
            {
                return NotFound();
            }

            var errorFecha = ValidarFechaSiembra(actualizarDto.FechaSiembra);
            if (errorFecha is not null)
            {
                return BadRequest(errorFecha);
            }

            if (!Enum.IsDefined(typeof(EstadoSiembra), actualizarDto.Estado))
            {
                return BadRequest("El estado de la siembra no es válido.");
            }

            var inicioDiaUtc = actualizarDto.FechaSiembra.Date.AddHours(5);
            if (await _context.RegistrosSeguimiento.AnyAsync(x =>
                    x.SiembraId == id && x.FechaHora < inicioDiaUtc)
                || await _context.ActividadesCuidado.AnyAsync(x =>
                    x.SiembraId == id && x.FechaHora < inicioDiaUtc)
                || await _context.Cosechas.AnyAsync(x =>
                    x.SiembraId == id && x.FechaHora < inicioDiaUtc))
            {
                return BadRequest("La fecha de siembra no puede ser posterior a su historial de registros.");
            }

            if (actualizarDto.Estado == EstadoSiembra.Cosechada
                && !await _context.Cosechas.AnyAsync(x => x.SiembraId == id))
            {
                return Conflict("Para marcar la siembra como cosechada debe existir al menos una cosecha.");
            }

            siembra.FechaSiembra = actualizarDto.FechaSiembra.Date;
            siembra.CantidadPlantasInicial = actualizarDto.CantidadPlantasInicial;
            siembra.Estado = actualizarDto.Estado;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSiembra(int id)
        {
            var siembra = await SiembrasAccesibles().FirstOrDefaultAsync(s => s.Id == id);
            if (siembra == null)
            {
                return NotFound();
            }

            if (await _context.RegistrosSeguimiento.AnyAsync(x => x.SiembraId == id)
                || await _context.ActividadesCuidado.AnyAsync(x => x.SiembraId == id)
                || await _context.Cosechas.AnyAsync(x => x.SiembraId == id))
            {
                return Conflict("No se puede eliminar una siembra que tiene registros en su historial.");
            }

            _context.Siembras.Remove(siembra);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se pudo eliminar la siembra. Comprueba sus registros relacionados.");
            }

            return NoContent();
        }

        private static string? ValidarFechaSiembra(DateTime fecha)
        {
            if (fecha.Date == default)
            {
                return "La fecha de siembra es obligatoria.";
            }

            var hoyColombia = DateTime.UtcNow.AddHours(-5).Date;
            if (fecha.Date > hoyColombia)
            {
                return "La fecha de siembra no puede ser futura.";
            }

            return null;
        }
    }
}
