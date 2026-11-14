using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.API.Helpers;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;
using Terpy.Shared.Enums;

namespace Terpy.API.Controllers;

[Route("api/cuidados")]
[ApiController]
[Authorize]
public class CuidadosController : ControllerBase
{
    private readonly DataContext _context;

    public CuidadosController(DataContext context)
    {
        _context = context;
    }

    private IQueryable<Siembra> SiembrasAccesibles()
    {
        var siembras = _context.Siembras.AsQueryable();
        if (User.EsAdministrador())
        {
            return siembras;
        }

        var usuarioId = User.ObtenerUsuarioId();
        return siembras.Where(s => _context.Zonas.Any(z =>
            z.Id == s.ZonaId && _context.Huertas.Any(h =>
                h.Id == z.HuertaId && h.ResponsableId == usuarioId)));
    }

    private IQueryable<ActividadCuidado> CuidadosAccesibles()
    {
        var siembras = SiembrasAccesibles();
        return _context.ActividadesCuidado.Where(c =>
            siembras.Any(s => s.Id == c.SiembraId));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActividadCuidado>>> GetCuidados()
    {
        var cuidados = await CuidadosAccesibles().AsNoTracking().ToListAsync();
        foreach (var cuidado in cuidados)
        {
            cuidado.FechaHora = DateTime.SpecifyKind(cuidado.FechaHora, DateTimeKind.Utc);
        }

        return Ok(cuidados);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActividadCuidado>> GetCuidado(int id)
    {
        var cuidado = await CuidadosAccesibles().AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cuidado == null)
        {
            return NotFound();
        }

        cuidado.FechaHora = DateTime.SpecifyKind(cuidado.FechaHora, DateTimeKind.Utc);
        return Ok(cuidado);
    }

    [HttpPost]
    public async Task<ActionResult<ActividadCuidado>> PostCuidado(CrearActividadCuidadoDto dto)
    {
        var siembra = await SiembrasAccesibles().AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.SiembraId);
        if (siembra == null)
        {
            return NotFound();
        }

        if (siembra.Estado != EstadoSiembra.Activa)
        {
            return BadRequest("No se pueden añadir cuidados a una siembra que no está Activa.");
        }

        if (string.IsNullOrWhiteSpace(dto.Descripcion))
        {
            return BadRequest("La descripción no puede estar vacía.");
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        var cuidado = new ActividadCuidado
        {
            SiembraId = dto.SiembraId,
            FechaHora = dto.FechaHora.ToUniversalTime(),
            TipoActividad = dto.TipoActividad,
            Descripcion = dto.Descripcion.Trim()
        };

        _context.ActividadesCuidado.Add(cuidado);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCuidado), new { id = cuidado.Id }, cuidado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCuidado(int id, ActualizarActividadCuidadoDto dto)
    {
        var cuidado = await CuidadosAccesibles().FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.Descripcion))
        {
            return BadRequest("La descripción no puede estar vacía.");
        }

        var siembra = await SiembrasAccesibles().AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == cuidado.SiembraId);
        if (siembra == null)
        {
            return NotFound();
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        cuidado.FechaHora = dto.FechaHora.ToUniversalTime();
        cuidado.TipoActividad = dto.TipoActividad;
        cuidado.Descripcion = dto.Descripcion.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCuidado(int id)
    {
        var cuidado = await CuidadosAccesibles().FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado == null)
        {
            return NotFound();
        }

        _context.ActividadesCuidado.Remove(cuidado);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidarFecha(DateTime fechaHora, DateTime fechaSiembra)
    {
        if (fechaHora == default)
        {
            return "La fecha del cuidado es obligatoria.";
        }

        if (fechaHora.Kind == DateTimeKind.Unspecified)
        {
            return "Envía la fecha en UTC, terminada en Z, o con su zona horaria.";
        }

        var fechaUtc = fechaHora.ToUniversalTime();
        if (fechaUtc > DateTime.UtcNow)
        {
            return "La fecha del cuidado no puede ser futura.";
        }

        // El día de siembra comienza a las 05:00 UTC en Colombia.
        if (fechaUtc < fechaSiembra.Date.AddHours(5))
        {
            return "La fecha del cuidado no puede ser anterior al día de siembra.";
        }

        return null;
    }
}
