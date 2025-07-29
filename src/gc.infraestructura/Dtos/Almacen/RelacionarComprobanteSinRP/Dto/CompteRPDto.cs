using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;

namespace gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP
{
	public class CompteRPDto : RprAsociadosDto
	{
		public char justificado { get; set; }

		private bool _justificado_bool;

		public bool justificado_bool
		{
			get { return justificado == 'N' ? false : true; }
			set { _justificado_bool = value; }
		}

		private string _concepto { get; set; } = string.Empty;
		public string concepto
		{
			get { return $"{tco_desc_rp} ({tco_id_rp}) {cm_compte_rp}"; }
			set { _concepto = value ?? string.Empty; }
		}	
	}
}
