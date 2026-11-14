using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.API.Helpers;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;
using Terpy.Shared.Enums;

namespace Terpy.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cosechas")]
public class CosechasController : ControllerBase
{
    private readonly DataContext _context;

    public CosechasController(DataContext context)
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

    private IQueryable<Cosecha> RegistrosAccesibles()
    {
        var siembras = SiembrasAccesibles();
        return _context.Cosechas.Where(x =>
            siembras.Any(s => s.Id == x.SiembraId));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cosecha>>> Get()
    {
        var cosechas = await RegistrosAccesibles()
            .AsNoTracking()
            .ToListAsync();

        foreach (var registro in cosechas)
        {
            registro.FechaHora = DateTime.SpecifyKind(registro.FechaHora, DateTimeKind.Utc);
        }

        return Ok(cosechas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cosecha>> GetById(int id)
    {
        var cosecha = await RegistrosAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cosecha is null)
        {
            return NotFound();
        }

        cosecha.FechaHora = DateTime.SpecifyKind(cosecha.FechaHora, DateTimeKind.Utc);
        return Ok(cosecha);
    }

    [HttpPost]
    public async Task<ActionResult<Cosecha>> Post(CrearCosechaDto dto)
    {
        var siembra = await SiembrasAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.SiembraId);

        if (siembra is null)
        {
            return NotFound();
        }

        if (siembra.Estado != EstadoSiembra.Activa)
        {
            return BadRequest("Solo se pueden registrar cosechas en siembras activas.");
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        var cosecha = new Cosecha
        {
            SiembraId = dto.SiembraId,
            FechaHora = dto.FechaHora.ToUniversalTime(),
            CantidadKg = dto.CantidadKg,
            Observacion = dto.Observacion
        };

        _context.Cosechas.Add(cosecha);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cosecha.Id }, cosecha);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, ActualizarCosechaDto dto)
    {
        var cosecha = await RegistrosAccesibles()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cosecha is null)
        {
            return NotFound();
        }

        var siembra = await SiembrasAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == cosecha.SiembraId);

        if (siembra is null)
        {
            return NotFound();
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        cosecha.FechaHora = dto.FechaHora.ToUniversalTime();
        cosecha.CantidadKg = dto.CantidadKg;
        cosecha.Observacion = dto.Observacion;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cosecha = await RegistrosAccesibles()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cosecha is null)
        {
            return NotFound();
        }

        var siembra = await SiembrasAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == cosecha.SiembraId);

        if (siembra is null)
        {
            return NotFound();
        }

        if (siembra.Estado == EstadoSiembra.Cosechada
            && !await _context.Cosechas.AnyAsync(x =>
                x.SiembraId == cosecha.SiembraId && x.Id != cosecha.Id))
        {
            return Conflict("No se puede eliminar la última cosecha de una siembra cosechada.");
        }

        _context.Cosechas.Remove(cosecha);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static string? ValidarFecha(DateTime fechaHora, DateTime fechaSiembra)
    {
        if (fechaHora == default)
        {
            return "La fecha del registro es obligatoria.";
        }

        if (fechaHora.Kind == DateTimeKind.Unspecified)
        {
            return "Envía la fecha en UTC, terminada en Z, o con su zona horaria.";
        }

        var fechaUtc = fechaHora.ToUniversalTime();
        if (fechaUtc > DateTime.UtcNow)
        {
            return "La fecha del registro no puede ser futura.";
        }

        // El día de siembra comienza a las 05:00 UTC en Colombia.
        if (fechaUtc < fechaSiembra.Date.AddHours(5))
        {
            return "La fecha del registro no puede ser anterior al día de siembra.";
        }

        return null;
    }
}
