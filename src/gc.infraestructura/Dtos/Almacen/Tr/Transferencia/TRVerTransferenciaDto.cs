using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRVerTransferenciaDto : Dto
	{
		public GridCore<TRVerConteosDto> ListaTransferencias { get; set; }
        public string Autorizacion { get; set; } = string.Empty;
		public string TipoTR { get; set; } = string.Empty;
		public string Tipo { get; set; } = string.Empty;
		public string Destino { get; set; } = string.Empty;
        public string ti { get; set; }
        public TRVerTransferenciaDto()
		{
			ListaTransferencias = new GridCore<TRVerConteosDto>();
		}
	}
}
