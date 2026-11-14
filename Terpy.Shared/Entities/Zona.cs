using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.Entities;

public class Zona
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int HuertaId { get; set; }

    [Required, MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }
}
