using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.DTOs
{
    public class CrearHuertaDto
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [MaxLength(80, ErrorMessage = "El campo no puede tener más de 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [MaxLength(200, ErrorMessage = "El campo no puede tener más de 200 caracteres.")]
        public string Ubicacion { get; set; } = string.Empty;
    }
}
