using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiOfertaServicio: Servicio<EntidadBase>, IApiOfertaServicio
    {
        public ApiOfertaServicio(IUnitOfWork uow) : base(uow)
        {

        }
    }
}
