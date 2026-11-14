using System.ComponentModel.DataAnnotations;
using Terpy.Shared.Enums;

namespace Terpy.Shared.Entities;

public class Siembra
{
    public int Id { get; set; }
    [Range(1, int.MaxValue)]
    public int ZonaId { get; set; }
    [Range(1, int.MaxValue)]
    public int CultivoId { get; set; }
    [DataType(DataType.Date)]
    public DateTime FechaSiembra { get; set; }
    [Range(1, int.MaxValue)]
    public int? CantidadPlantasInicial { get; set; }
    [EnumDataType(typeof(EstadoSiembra))]
    public EstadoSiembra Estado { get; set; } = EstadoSiembra.Activa;
}