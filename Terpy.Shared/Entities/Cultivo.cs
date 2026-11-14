using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.Entities;

public class Cultivo
{
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
