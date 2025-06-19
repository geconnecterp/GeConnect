
namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPRComptesDeRPDto : Dto
	{
		public string Tipo { get; set; }=string.Empty;
        public string TipoDescripcion { get; set; } = string.Empty;
        public string NroComprobante { get; set; } = string.Empty;
        public string Fecha { get; set; }= string.Empty;
        public string Importe { get; set; }=string.Empty ;
		public decimal Importe2 { get; set; } = 0.00M;
		public string Rp { get; set; } = string.Empty;
    }
}
