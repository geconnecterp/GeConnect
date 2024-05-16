using gc.api.core.Entidades;
using gc.infraestructura.Core.Responses;

namespace gc.api.core.Contratos.Servicios
{
    public interface IBilleteraOrdenServicio : IServicio<BilleteraOrden>
    {
        (bool,string) CargarOrden(BilleteraOrden orden);
        (bool, string) OrdenNotificado(OrdenNotificado ordenNotificado);
        (bool, string) OrdenRegistro(OrdenRegistro ordenRegistro);
        (bool, string) VerificaPago(string ordenId);
    }
}
