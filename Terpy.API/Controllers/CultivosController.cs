using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;

namespace Terpy.API.Controllers;

[ApiController]
[Route("api/cultivos")]
public class CultivosController : ControllerBase
{
    private readonly DataContext _context;

    public CultivosController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Cultivo>>> GetCultivos()
    {
        var cultivos = await _context.Cultivos.ToListAsync();
        return Ok(cultivos);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Cultivo>> GetCultivo(int id)
    {
        var cultivo = await _context.Cultivos.FindAsync(id);
        if (cultivo == null)
        {
            return NotFound();
        }
        return Ok(cultivo);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<Cultivo>> PostCultivo(CrearCultivoDto crearDto)
    {
        var nombreTrim = crearDto.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreTrim))
        {
            return BadRequest("El nombre no puede estar vacío.");
        }

        var existe = await _context.Cultivos
            .AnyAsync(c => c.Nombre.ToLower() == nombreTrim.ToLower());
        if (existe)
        {
            return Conflict("Ya existe un cultivo con ese nombre.");
        }

        var cultivo = new Cultivo
        {
            Nombre = nombreTrim,
            Descripcion = string.IsNullOrWhiteSpace(crearDto.Descripcion) ? null : crearDto.Descripcion.Trim()
        };

        _context.Cultivos.Add(cultivo);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Error al guardar el cultivo, posible duplicado.");
        }

        return CreatedAtAction(nameof(GetCultivo), new { id = cultivo.Id }, cultivo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> PutCultivo(int id, ActualizarCultivoDto actualizarDto)
    {
        var cultivo = await _context.Cultivos.FindAsync(id);
        if (cultivo == null)
        {
            return NotFound();
        }

        var nombreTrim = actualizarDto.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreTrim))
        {
            return BadRequest("El nombre no puede estar vacío.");
        }

        var existe = await _context.Cultivos
            .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == nombreTrim.ToLower());
        if (existe)
        {
            return Conflict("Ya existe otro cultivo con ese nombre.");
        }

        cultivo.Nombre = nombreTrim;
        cultivo.Descripcion = string.IsNullOrWhiteSpace(actualizarDto.Descripcion) ? null : actualizarDto.Descripcion.Trim();

        _context.Entry(cultivo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cultivos.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteCultivo(int id)
    {
        var cultivo = await _context.Cultivos.FindAsync(id);
        if (cultivo == null)
        {
            return NotFound();
        }

        var tieneSiembras = await _context.Siembras.AnyAsync(s => s.CultivoId == id);
        if (tieneSiembras)
        {
            return Conflict("No se puede eliminar el cultivo porque tiene siembras asociadas.");
        }

        _context.Cultivos.Remove(cultivo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}