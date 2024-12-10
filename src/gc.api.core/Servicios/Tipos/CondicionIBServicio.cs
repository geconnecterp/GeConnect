using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
    public class CondicionIBServicio : Servicio<CondicionIB>, ICondicionIBServicio
    {
        public CondicionIBServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<CondicionIBDto> GetCondicionIBLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CONDICION_IB_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new CondicionIBDto()
                {
                    #region Campos
                    id_id = x.Ib_id,
                    ib_desc = x.Ib_desc,
                    ib_lista = x.Ib_lista,
                    #endregion
                }).ToList();
        }
    }
}
