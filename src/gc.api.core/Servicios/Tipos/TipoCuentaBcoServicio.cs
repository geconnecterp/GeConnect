using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoCuentaBcoServicio : Servicio<TipoCuentaBco>, ITipoCuentaBcoServicio
    {
        public TipoCuentaBcoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<TipoCuentaBcoDto> GetTipoCuentaBcoLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CUENTA_BCO_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new TipoCuentaBcoDto()
                {
                    #region Campos
                    tcb_id = x.Tcb_id,
                    tcb_desc = x.Tcb_desc,
                    tcb_lista = x.Tcb_lista,
                    #endregion
                }).ToList();
        }
    }
}
