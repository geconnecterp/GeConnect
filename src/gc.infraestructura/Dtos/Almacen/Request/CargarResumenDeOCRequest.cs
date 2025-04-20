
namespace gc.infraestructura.Dtos.Almacen.Request
{
    public class CargarResumenDeOCRequest : ActualizarConceptosRequest
	{
		public string Cta_Id { get; set; } = string.Empty;
		public string Adm_Id { get; set; } = string.Empty;
		public string Usu_Id { get; set; } = string.Empty;
		
		public string Json { get; set; } = string.Empty;
	}

	public class ActualizarConceptosRequest
	{
		public bool Nueva { get; set; }
		public string Oc_Compte { get; set; } = string.Empty;
		public DateTime Entrega_Fecha { get; set; }
		public string Entrega_Adm { get; set; } = string.Empty;
		public char Pago_Anticipado { get; set; }
		public DateTime Pago_Fecha { get; set; }
		public string Observaciones { get; set; } = string.Empty;
		public char Oce_Id { get; set; }
	}
}
