using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
    public class BuscarCuentaDto
    {
        public string Cuenta { get; set; } = string.Empty;
		public SelectList ComboDeposito { get; set; }

	}
}
