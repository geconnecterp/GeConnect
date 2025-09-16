using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LibroBancoDetalleModel
	{
		public string Descripcion_Grid_Cero { get; set; } = string.Empty;
		public GridCoreSmart<FinancieroBcoLibroDto> GrillaBcoLibro_Cero { get; set; }
		public string Descripcion_Grid_Uno { get; set; } = string.Empty;
		public GridCoreSmart<FinancieroBcoLibroDto> GrillaBcoLibro_Uno { get; set; }
		public string Descripcion_Grid_Dos { get; set; } = string.Empty;
		public GridCoreSmart<FinancieroBcoLibroDto> GrillaBcoLibro_Dos { get; set; }
		public string saldo_bco { get; set; } = string.Empty;
		public string saldo_bco_descripcion { get; set; } = string.Empty;
		public string saldo_bco_che { get; set; } = string.Empty;
		public string saldo_bco_che_descripcion { get; set; } = string.Empty;
		public string saldo_pendiente { get; set; } = string.Empty;
		public string saldo_pendiente_descripcion { get; set; } = string.Empty;
		public string conciliado_m_ant { get; set; } = string.Empty;
		public string conciliado_m_ant_descripcion { get; set; } = string.Empty;
		public string conciliado_m_sig { get; set; } = string.Empty;
		public string conciliado_m_sig_descripcion { get; set; } = string.Empty;
		public string conciliado_m_pos { get; set; } = string.Empty;
		public string conciliado_m_pos_descripcion { get; set; } = string.Empty;
	}
}
