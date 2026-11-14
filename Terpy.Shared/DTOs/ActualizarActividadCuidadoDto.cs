using System.ComponentModel.DataAnnotations;
using Terpy.Shared.Enums;

namespace Terpy.Shared.DTOs;

public class ActualizarActividadCuidadoDto
{
    

    public DateTime FechaHora { get; set; }

	[EnumDataType(typeof(TipoActividad))]
	public TipoActividad TipoActividad { get; set; }

	[Required, MaxLength(1000)]
	public string Descripcion { get; set; } = string.Empty;
}