using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs;

public class ActualizarCosechaDto
{
    public DateTime FechaHora { get; set; }

    [Range(
        typeof(decimal),
        "0.001",
        "9999999.999",
        ParseLimitsInInvariantCulture = true)]
    public decimal CantidadKg { get; set; }

    [MaxLength(1000)]
    public string? Observacion { get; set; }
}
