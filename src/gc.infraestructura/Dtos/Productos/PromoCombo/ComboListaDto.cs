using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace gc.infraestructura.Dtos.Productos.PromoCombo
{
    public class ComboListaDto : ComboDatosDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
    }

    public class ComboDatosDto
    {
        public string cmb_id { get; set; } = string.Empty;
        public string cmb_desc { get; set; } = string.Empty;
        public char cmb_tipo { get; set; }
        public string cmb_tipo_desc { get; set; } = string.Empty;
        public char cmb_estado { get; set; }
        public string cmb_estados_desc { get; set; } = string.Empty;
        public DateTime cmb_desde { get; set; }
        public DateTime cmb_hasta { get; set; }
        public bool pasa_activar { get; set; }
        public bool pasa_historico { get; set; }
    }

    public class ConfirmacionRequestDto
    {
        public ComboDatosDto Datos { get; set; } = new();
        public List<ComboCanalDto> Canales { get; set; } = [];
        public List<ComboProductoDto> Productos { get; set; } = [];
    }

    public class ComboRepoDto
    {
        public string cmb_id { get; set; }= string.Empty;
        public string p_id { get; set; }= string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal cantidad { get; set; }
        public decimal dto_porc { get; set; }
        public decimal dto_imp { get; set; }
        public string cmb_desc { get; set; } = string.Empty;
        public DateTime  cmb_carga { get; set; }
        public string usu_id { get; set; } = string.Empty;
        public DateTime cmb_desde { get; set; }
        public DateTime cmb_hasta { get; set; }
        public char cmb_actu { get; set; }
        public DateTime cmb_fecha { get; set; }
        public char cmb_tipo { get; set; }
        public string adm_id { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string lp_desc { get; set; } = string.Empty;
        public string p_id_sustituto { get; set; } = string.Empty;
        public string p_desc_sustituto { get; set; } = string.Empty;






    }
}
