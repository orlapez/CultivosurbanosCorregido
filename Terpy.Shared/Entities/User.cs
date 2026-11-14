using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Terpy.Shared.Entities;

public class User : IdentityUser
{
    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Apellido { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FotoUrl { get; set; }
}
