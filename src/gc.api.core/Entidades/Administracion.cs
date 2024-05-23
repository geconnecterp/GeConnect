using System.ComponentModel.DataAnnotations.Schema;

namespace gc.api.core.Entidades
{
    
    public partial class Administracion : EntidadBase
    {
        public Administracion()
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

        public string Adm_id { get; set; }
        public string Adm_nombre { get; set; }
        public string Adm_direccion { get; set; }
        public string Usu_id_encargado { get; set; }
        public int Adm_nro_lote { get; set; }
        public int Adm_nro_lote_central { get; set; }
        public char Adm_central { get; set; }
        public char Cx_existe { get; set; }
        public string Cx_profile { get; set; }
        public string Cx_base { get; set; }
        public string Cx_login { get; set; }
        public string Cx_pass { get; set; }
        public DateTime? Lote_f { get; set; }
        public DateTime? Lote_central_f { get; set; }
        public char Adm_activa { get; set; }
        public decimal Adm_oc_limite { get; set; }
        public string Adm_MePa_Id { get; set; }


        //public virtual ICollection<depositos> depositoss { get; set; }

    }
}
