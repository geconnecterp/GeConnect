using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Almacen
{
    public class ProductoListaDto : Dto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
        public string P_id { get; set; } = string.Empty;//+
        public string P_id_barrado { get; set; } = string.Empty;
        public string P_id_prov { get; set; } = string.Empty;
        public string P_desc { get; set; } = string.Empty;//+
        public string Rub_id { get; set; } = string.Empty;
        public string Rub_desc { get; set; } = string.Empty;
        [Display(Name = "Rub Lista")]
        public string Rub_lista { get; set; } = string.Empty;
        public string Rubg_id { get; set; } = string.Empty;//+
        public string Rubg_desc { get; set; } = string.Empty;//+
        public string Cta_id { get; set; } = string.Empty;//+
        public string Cta_denominacion { get; set; } = string.Empty;//+
        [Display(Name = "Lista")]
        public string Cta_lista { get; set; } = string.Empty;
        public string P_activo { get; set; } = string.Empty;//+
        public string P_activo_des { get; set; } = string.Empty;//+
        public string P_pneto { get; set; } = string.Empty;
        public string P_pvta { get; set; } = string.Empty;
        public string P_pcosto { get; set; } = string.Empty;
        public string P_pcosto_repo { get; set; } = string.Empty;
        public string Ps_stk { get; set; } = string.Empty;

    }
}
