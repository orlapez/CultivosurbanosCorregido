using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs;

public class ActualizarCultivoDto
{
    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
