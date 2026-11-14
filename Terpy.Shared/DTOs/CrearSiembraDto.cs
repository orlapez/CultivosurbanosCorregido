using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs;

public class CrearSiembraDto
{
    [Range(1, int.MaxValue)]
    public int ZonaId { get; set; }
    [Range(1, int.MaxValue)]
    public int CultivoId { get; set; }
    [DataType(DataType.Date)]
    public DateTime FechaSiembra { get; set; }
    [Range(1, int.MaxValue)]
    public int? CantidadPlantasInicial { get; set; }
}
