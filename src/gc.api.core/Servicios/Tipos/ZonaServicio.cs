using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class ZonaServicio : Servicio<Zona>, IZonaServicio
    {
        public ZonaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<ZonaDto> GetZonaLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ZONAS_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new ZonaDto()
                {
                    #region Campos
                    zn_id= x.zn_id,
                    zn_desc= x.zn_desc,
                    zn_lista= x.zn_lista,
                    #endregion
                }).ToList();
        }
    }
}
