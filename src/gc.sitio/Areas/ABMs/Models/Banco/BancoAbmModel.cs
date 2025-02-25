using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class BancoAbmModel
	{
		public BancoDto Banco { get; set; }
		public SelectList Moneda { get; set; }
		public SelectList TipoCuenta { get; set; }
		public SelectList CuentaGasto { get; set; }
		public SelectList CuentaContable { get; set; }
		public BancoAbmModel() 
		{
			Banco = new BancoDto();
		}
	}
}
