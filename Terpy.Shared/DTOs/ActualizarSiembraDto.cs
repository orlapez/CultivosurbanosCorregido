using System.ComponentModel.DataAnnotations;
using Terpy.Shared.Enums;

namespace Terpy.Shared.DTOs;

public class ActualizarSiembraDto
{
    [DataType(DataType.Date)]
    public DateTime FechaSiembra { get; set; }
    [Range(1, int.MaxValue)]
    public int? CantidadPlantasInicial { get; set; }
    [EnumDataType(typeof(EstadoSiembra))]
    public EstadoSiembra Estado { get; set; }
}