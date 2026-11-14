
using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.Entities;

public class RegistroSeguimiento : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int SiembraId { get; set; }

    public DateTime FechaHora { get; set; }

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
