namespace gc.sitio.Areas.Compras.Models.RecepcionDeProveedores.Request
{
	public class GuardarDetalleDeComprobanteRpRequest
	{
		public bool guardado { get; set; }
		public bool generar { get; set; }
		public string listaProd { get; set; }
		public bool ponerEnCurso { get; set; }
		public string ulCantidad { get; set; }
		public string fechaTurno { get; set; }
		public string depoId { get; set; }
		public string nota { get; set; }
	}
}
