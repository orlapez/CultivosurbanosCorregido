using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs;

public class ActualizarRegistroSeguimientoDto : IValidatableObject
{
    public DateTime FechaHora { get; set; }

    [Range(typeof(decimal), "-999.99", "999.99", ParseLimitsInInvariantCulture = true)]
    public decimal? TemperaturaC { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? HumedadSueloPorcentaje { get; set; }

    [MaxLength(1000)]
    public string? Observacion { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (TemperaturaC is null
            && HumedadSueloPorcentaje is null
            && string.IsNullOrWhiteSpace(Observacion))
        {
            yield return new ValidationResult(
                "Registra una observación, una temperatura o una humedad.",
                new[]
                {
                    nameof(Observacion),
                    nameof(TemperaturaC),
                    nameof(HumedadSueloPorcentaje)
                });
        }
    }
}
