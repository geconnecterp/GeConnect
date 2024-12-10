using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoNegocioServicio : Servicio<TipoNegocio>, ITipoNegocioServicio
    {
        public TipoNegocioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<TipoNegocioDto> GetTiposDeNegocio()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPOS_NEGOCIO_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new TipoNegocioDto()
                {
                    #region Campos
                    ctn_id = x.ctn_id,
                    ctn_desc = x.ctn_desc,
                    ctn_lista = x.ctn_lista,
                    #endregion
                }).ToList();
            
        }
    }
}
