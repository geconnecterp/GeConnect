using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Seguridad
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El {0} debe ser ingresado para poder acceder.")]
        [Display(Name = "Usuario")]
        [UIHint("_User")]
        public string? UserName { get; set; }

        [Display(Name = "Correo electrónico")]
        [UIHint("_Text")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [UIHint("_Pwd")]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }
    }
}
