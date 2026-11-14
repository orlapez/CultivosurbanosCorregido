using System.ComponentModel.DataAnnotations;
using Terpy.Shared.Enums;

namespace Terpy.Shared.Entities;

public class ActividadCuidado
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int SiembraId { get; set; }

    public DateTime FechaHora { get; set; }

    [EnumDataType(typeof(TipoActividad))]
    public TipoActividad TipoActividad { get; set; }

    [Required, MaxLength(1000)]
    public string Descripcion { get; set; } = string.Empty;
}

