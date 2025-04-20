using gc.infraestructura.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace gc.infraestructura.Dtos.Seguridad
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El {0} debe ser ingresado para poder acceder.")]
        [Display(Name = "Usuario")]
        [UIHint("_User")]
        public string? UserName { get; set; }

        //[Display(Name = "Correo electrónico")]
        //[UIHint("_Text")]
        //[EmailAddress]
        //public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [UIHint("_Pwd")]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Administración")]
        public string Admid { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
        public string FechaNN { get { return Fecha.ToString("g", System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")); } }
    }
}
