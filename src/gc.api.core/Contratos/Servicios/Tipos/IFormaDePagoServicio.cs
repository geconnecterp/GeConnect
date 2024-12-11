using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface IFormaDePagoServicio : IServicio<FormaDePago>
    {
        List<FormaDePagoDto> GetFormaDePagoLista(string tipo = "C");
    }
}
