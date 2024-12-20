using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoCanalServicio : Servicio<TipoCanal>, ITipoCanalServicio
    {
        public TipoCanalServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<TipoCanalDto> GetTipoCanalLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CANAL_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new TipoCanalDto()
                {
                    #region Campos
                    ctc_id = x.Ctc_id,
                    ctc_desc = x.Ctc_desc,
                    ctc_lista = x.Ctc_lista,
                    #endregion
                }).ToList();
        }
    }
}
