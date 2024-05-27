using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Administracion;

namespace gc.api.core.Contratos.Servicios
{
    public interface IAdministracionServicio : IServicio<Administracion>
    {
        bool ActualizaMePaId(AdmUpdateMePaDto datos);
    }
}
