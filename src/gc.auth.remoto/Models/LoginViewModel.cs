using gc.infraestructura.Dtos.Administracion;
using System.ComponentModel.DataAnnotations;

namespace gc.auth.remoto.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Debe ingresar el usuario.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar la contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar una administración.")]
    [Display(Name = "Administración")]
    public string Admid { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;

    public IReadOnlyList<AdministracionLoginDto> Administraciones { get; set; }
        = Array.Empty<AdministracionLoginDto>();
}
