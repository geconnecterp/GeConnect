using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Rpr;

namespace gc.api.core.Contratos.Servicios
{
    public interface IAdministracionServicio : IServicio<Administracion>
    {
        bool ActualizaMePaId(AdmUpdateMePaDto datos);

        ResponseBaseDto ValidaUsuario(string tipo, string id, string usuId);
        List<AdministracionDto> ObtenerAdministraciones(string adm_activa = "%");
    }
}
