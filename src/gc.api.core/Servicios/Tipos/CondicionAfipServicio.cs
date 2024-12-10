using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;


namespace gc.api.core.Servicios
{
    public class CondicionAfipServicio : Servicio<CondicionAfip>, ICondicionAfipServicio
    {
        public CondicionAfipServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<CondicionAfipDto> GetCondicionesAfipLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CONDICION_AFIP_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new CondicionAfipDto()
                {
                    #region Campos
                    afip_id = x.Afip_id,
                    afip_desc = x.Afip_desc,
                    afip_desc_sort = x.Afip_desc_sort,
                    afip_lista = x.Afip_lista,
                    #endregion
                }).ToList();
        }
    }
}
