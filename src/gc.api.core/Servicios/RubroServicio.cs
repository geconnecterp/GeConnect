using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class RubroServicio : Servicio<Rubro>, IRubroServicio
    {
        public RubroServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public List<RubroListaDto> GetRubroLista(string cta_id="%")
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RUBRO_LISTA;
            var ps = new List<SqlParameter>() { new SqlParameter("@cta_id",cta_id) };
            var res = _repository.EjecutarLstSpExt<RubroListaDto>(sp, ps, true);

            if (res.Count == 0)
            {
                return new List<RubroListaDto>();
            }
            else
            {
                return res.ToList();
            }
        }
      
    }
}
