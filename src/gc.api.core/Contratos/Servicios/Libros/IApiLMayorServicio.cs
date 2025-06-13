using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros
{
    public interface IApiLMayorServicio:IServicio<EntidadBase>
    {
        List<LMayorRegListaDto> ObtenerLibroMayor(int nro_eje,string ccb_id, bool fa, DateTime fecha_desde, DateTime fecha_hasta, bool conTemporales, int regs, int pag, string ord);
        
    }
}
