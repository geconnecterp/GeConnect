
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos
{
	public class RemitoNoFiscalDto : Dto, IProductoConUnidad
	{
		public string emisor_nombre { get; set; } = string.Empty;
		public string emisor_cuit { get; set; } = string.Empty;
		public string emisor_domicilio { get; set; } = string.Empty;
		public string emisor_ib_nro { get; set; } = string.Empty;
		public DateTime emisor_fecha_ini { get; set; }
		public string emidosr_afip_id { get; set; } = string.Empty;
		public string emisor_afip_desc { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string afip_desc { get; set; } = string.Empty;
		public string cai { get; set; } = string.Empty;
		public DateTime cai_vto { get; set; }
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string sm_tipo { get; set; } = string.Empty;
		public string sm_compte { get; set; } = string.Empty;
		public string cm_compte_ctacte { get; set; } = string.Empty;
		public string tco_id_ori { get; set; } = string.Empty;
		public string cm_compte_ori { get; set; } = string.Empty;
		public string pre_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string obs { get; set; } = string.Empty;
		public int item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_id_desc { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public int bultos { get; set; }
		public decimal us { get; set; }
		public decimal cantidad { get; set; }
		public decimal costo { get; set; }
		public decimal costo_tot { get; set; }
		public bool PermiteDecimales => up_tipo == "P";
	}
}
