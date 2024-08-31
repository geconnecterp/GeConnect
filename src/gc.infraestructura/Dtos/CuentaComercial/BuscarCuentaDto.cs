using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
    public class BuscarCuentaDto
    {
        public string Cuenta { get; set; } = string.Empty;
		public SelectList ComboDeposito { get; set; }
        public string IdTipoCompte { get; set; } = string.Empty;
        public string NroCompte { get; set; }= string.Empty;

    }
}
