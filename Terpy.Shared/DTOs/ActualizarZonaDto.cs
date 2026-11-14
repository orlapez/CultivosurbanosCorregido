using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs
{
    public class ActualizarZonaDto
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [MaxLength(60, ErrorMessage = "El campo no puede tener más de 60 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "El campo no puede tener más de 300 caracteres.")]
        public string? Descripcion { get; set; }
    }
}