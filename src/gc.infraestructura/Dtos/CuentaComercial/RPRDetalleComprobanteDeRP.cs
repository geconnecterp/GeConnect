﻿
namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPRDetalleComprobanteDeRP
	{
		public string Leyenda { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public RPRComptesDeRPDto CompteSeleccionado { get; set; } = new RPRComptesDeRPDto();

	}
}