using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
    public class RPRNuevaAutorizaciónDto
    {
        public SelectList ComboDeposito { get; set; } = new SelectList(new List<Dto>());
    }
}
