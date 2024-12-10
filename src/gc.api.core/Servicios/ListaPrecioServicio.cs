using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class ListaPrecioServicio : Servicio<ListaPrecio>, IListaPrecioServicio
    {
        public ListaPrecioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public List<ListaPrecioDto> GetListaPrecio()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_LISTA_PRECIO;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new ListaPrecioDto()
                {
                    #region Campos
                    lp_id = x.Lp_id,
                    lp_desc = x.Lp_desc,
                    lp_lista = x.Lp_lista,
                    #endregion
                }).ToList();
        }
    }
}
