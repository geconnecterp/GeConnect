
namespace gc.api.core.Entidades
{
	public class ProveedorGrupo : EntidadBase
	{
		public string cta_id { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public string pg_lista { get; set; } = string.Empty;
		public DateTime? pg_fecha_carga_precios { get; set; }
		public DateTime? pg_fecha_consulta_precios { get; set; }
		public DateTime? pg_fecha_cambio_precios { get; set; }
		public string pg_observaciones { get; set; } = string.Empty;
		public DateTime? pg_actu_fecha { get; set; }
		public char? pg_actu { get; set; }
	}
}
