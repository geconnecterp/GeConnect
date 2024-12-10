using gc.api.core.Entidades;

namespace gc.api.core.Contratos.Servicios
{
    public interface ITipoMotivoServicio:IServicio<TipoMotivo>
    {
        List<TipoMotivo> ObtenerTiposMotivo();
    }
}
