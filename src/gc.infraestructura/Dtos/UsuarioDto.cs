using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos
{
    public partial class UsuarioDto:Dto
    {
        public UsuarioDto()
        {
            Usu_Id = string.Empty;
            Usu_Password = string.Empty;
            Usu_Apellidoynombre = string.Empty;
            Tdo_Codigo = string.Empty;
            Usu_Ducumento = string.Empty;
            Usu_Email = string.Empty;
            Usu_Celu = string.Empty;
            Usu_Pin = string.Empty;
            Cta_Id = string.Empty;
        }
        public string Usu_Id { get; set; }
        public string Usu_Password { get; set; }
        public string Usu_Apellidoynombre { get; set; }
        public bool Usu_Bloqueado { get; set; }
        public DateTime? Usu_Bloqueado_Fecha { get; set; }
        public short Usu_Intentos { get; set; }
        public bool Usu_Estalogeado { get; set; }
        public DateTime? Usu_Alta { get; set; }
        public bool? Usu_Expira { get; set; }
        public short? Usu_Dias_Expiracion { get; set; }
        public DateTime? Usu_Fecha_Expira_Inicio { get; set; }
        public string Tdo_Codigo { get; set; }
        public string Usu_Ducumento { get; set; }
        public string Usu_Email { get; set; }
        public string Usu_Celu { get; set; }
        public string Usu_Pin { get; set; }
        public string Cta_Id { get; set; }

    }
}
