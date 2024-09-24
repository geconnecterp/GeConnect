
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRCRUDDto : Dto
	{
		public GridCore<TRAutSucursalesDto> ListaAutSucursales { get; set; }
		public GridCore<TRAutPIDto> ListaAutPI { get; set; }
		public GridCore<TRAutPIDto> ListaPedidosIncluidos { get; set; }
        public GridCore<TRAutDepoDto> ListaDepositosDeEnvio { get; set; }
        public TRCRUDDto()
		{
			ListaAutSucursales = new GridCore<TRAutSucursalesDto>();
			ListaAutPI = new GridCore<TRAutPIDto>();
			ListaPedidosIncluidos = new GridCore<TRAutPIDto>();
			ListaDepositosDeEnvio = new GridCore<TRAutDepoDto>();
		}
	}
}
