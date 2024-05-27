using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Cajas;
namespace gc.api.core.Contratos.Servicios
{
    public interface ICajaServicio : IServicio<Caja>
    {
        bool ActualizaMePaId(CajaUpMePaId datos);
        Caja? Find(string sucId, string cajaId);
    }
}
