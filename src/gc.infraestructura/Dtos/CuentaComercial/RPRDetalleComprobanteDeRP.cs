
namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPRDetalleComprobanteDeRP
	{
		public string Leyenda { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public RPRComptesDeRPDto CompteSeleccionado { get; set; } = new RPRComptesDeRPDto();
        public string Nota { get; set; } = string.Empty;
		public string FechaTurno { get; set; } = string.Empty;
		public string Depo_id { get; set; } = string.Empty;
		public bool ponerEnCurso { get; set; }

		public string Ul_cantidad { get; set; } = string.Empty;
    }
}
