
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRCRUDDto : Dto
	{
		public GridCoreSmart<TRAutSucursalesDto> ListaAutSucursales { get; set; }
		public GridCoreSmart<TRAutPIDto> ListaPedidosSucursal { get; set; }
		public GridCoreSmart<TRAutPIDto> ListaPedidosIncluidos { get; set; }
        public GridCoreSmart<TRAutDepoDto> ListaDepositosDeEnvio { get; set; }
		public bool ConsiderarStockExistente { get; set; }
		public bool ModificarYSustituto { get; set; }
		public bool MaximoPalletXAuto { get; set; }
        public int MaximoPalletXAutoValor { get; set; }
        public TRCRUDDto()
		{
			ListaAutSucursales = new GridCoreSmart<TRAutSucursalesDto>();
			ListaPedidosSucursal = new GridCoreSmart<TRAutPIDto>();
			ListaPedidosIncluidos = new GridCoreSmart<TRAutPIDto>();
			ListaDepositosDeEnvio = new GridCoreSmart<TRAutDepoDto>();
			ConsiderarStockExistente = true;
			ModificarYSustituto = false;
			MaximoPalletXAuto = false;
			MaximoPalletXAutoValor = 10;
		}
	}
}
