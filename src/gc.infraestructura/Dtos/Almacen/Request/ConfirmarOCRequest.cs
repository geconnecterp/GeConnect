
namespace gc.infraestructura.Dtos.Almacen.Request
{
    public class ConfirmarOCRequest
    {
		public string Cta_Id { get; set; } = string.Empty;
		public string Adm_Id { get; set; } = string.Empty;
		public string Usu_Id { get; set; } = string.Empty;
		public bool Nueva { get; set; }
		public string Oc_Compte { get; set; } = string.Empty;
		public string Json { get; set; } = string.Empty;
	}
}
