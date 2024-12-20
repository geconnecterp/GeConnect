using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoObsServicio : Servicio<TipoObs>, ITipoObsServicio
    {
        public TipoObsServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<TipoObsDto> GetTiposDeObs(string tipo = "C")
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_OBS_LISTA;
            var ps = new List<SqlParameter>()
            {
                new("@tipo",tipo)
            };
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new TipoObsDto()
                {
                    #region Campos
                    to_cliente = x.to_cliente,
                    to_desc = x.to_desc,
                    to_id = x.to_id,
                    to_proveedor = x.to_proveedor
                    #endregion
                }).ToList();
        }
    }
}
