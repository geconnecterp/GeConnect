using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos.Administracion
{
    public partial class AdministracionDto:Dto
    {
        public AdministracionDto()
        {
            Adm_id = string.Empty;
            Adm_nombre = string.Empty;
            Adm_direccion = string.Empty;
            Usu_id_encargado = string.Empty;
            Cx_profile = string.Empty;
            Cx_base = string.Empty;
            Cx_login = string.Empty;
            Cx_pass = string.Empty;
        }
        public string Adm_id { get; set; } = string.Empty;
        public string Adm_nombre { get; set; } = string.Empty;
        public string Adm_direccion { get; set; } = string.Empty;
        public string Usu_id_encargado { get; set; } = string.Empty;        
        public int Adm_nro_lote { get; set; }
        public int Adm_nro_lote_central { get; set; }
        public char Adm_central { get; set; }
        public char Cx_existe { get; set; }
        public string? Cx_profile { get; set; }
        public string? Cx_base { get; set; }
        public string? Cx_login { get; set; }
        public string? Cx_pass { get; set; }
        public DateTime? Lote_f { get; set; }
        public DateTime? Lote_central_f { get; set; }
        public char Adm_activa { get; set; }
        public decimal Adm_oc_limite { get; set; }
        public string? Adm_MePa_Id { get; set; }
        public string Adm_Mepa_Localidad { get; set; }=string.Empty;
        public string Adm_Mepa_Provincia { get; set; } = string.Empty;
        public string Adm_Mepa_Latitud { get; set; } = string.Empty;
        public string Adm_Mepa_Longitud { get; set; } = string.Empty;
        public string Adm_Mepa_Calle { get; set; } = string.Empty;
        public int Adm_Mepa_Numero { get; set; }

    }
}
