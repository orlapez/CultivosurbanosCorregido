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
[Route("api/seguimientos")]
public class SeguimientosController : ControllerBase
{
    private readonly DataContext _context;

    public SeguimientosController(DataContext context)
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

    private IQueryable<RegistroSeguimiento> RegistrosAccesibles()
    {
        var siembras = SiembrasAccesibles();
        return _context.RegistrosSeguimiento.Where(x =>
            siembras.Any(s => s.Id == x.SiembraId));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegistroSeguimiento>>> Get()
    {
        var seguimientos = await RegistrosAccesibles()
            .AsNoTracking()
            .ToListAsync();

        foreach (var registro in seguimientos)
        {
            registro.FechaHora = DateTime.SpecifyKind(registro.FechaHora, DateTimeKind.Utc);
        }

        return Ok(seguimientos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegistroSeguimiento>> GetById(int id)
    {
        var seguimiento = await RegistrosAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (seguimiento is null)
        {
            return NotFound();
        }

        seguimiento.FechaHora = DateTime.SpecifyKind(seguimiento.FechaHora, DateTimeKind.Utc);
        return Ok(seguimiento);
    }

    [HttpPost]
    public async Task<ActionResult<RegistroSeguimiento>> Post(CrearRegistroSeguimientoDto dto)
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
            return BadRequest("Solo se pueden registrar seguimientos en siembras activas.");
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        var seguimiento = new RegistroSeguimiento
        {
            SiembraId = dto.SiembraId,
            FechaHora = dto.FechaHora.ToUniversalTime(),
            TemperaturaC = dto.TemperaturaC,
            HumedadSueloPorcentaje = dto.HumedadSueloPorcentaje,
            Observacion = dto.Observacion
        };

        _context.RegistrosSeguimiento.Add(seguimiento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = seguimiento.Id }, seguimiento);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, ActualizarRegistroSeguimientoDto dto)
    {
        var seguimiento = await RegistrosAccesibles()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (seguimiento is null)
        {
            return NotFound();
        }

        var siembra = await SiembrasAccesibles()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == seguimiento.SiembraId);

        if (siembra is null)
        {
            return NotFound();
        }

        var errorFecha = ValidarFecha(dto.FechaHora, siembra.FechaSiembra);
        if (errorFecha is not null)
        {
            return BadRequest(errorFecha);
        }

        seguimiento.FechaHora = dto.FechaHora.ToUniversalTime();
        seguimiento.TemperaturaC = dto.TemperaturaC;
        seguimiento.HumedadSueloPorcentaje = dto.HumedadSueloPorcentaje;
        seguimiento.Observacion = dto.Observacion;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var seguimiento = await RegistrosAccesibles()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (seguimiento is null)
        {
            return NotFound();
        }

        _context.RegistrosSeguimiento.Remove(seguimiento);
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
