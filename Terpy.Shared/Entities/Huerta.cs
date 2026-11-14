using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.Entities;

public class Huerta
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Ubicacion { get; set; } = string.Empty;

    [Required]
    public string ResponsableId { get; set; } = string.Empty;
}
