using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class VendedorServicio : Servicio<Vendedor>, IVendedorServicio
    {
        public VendedorServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public List<VendedorDto> GetVendedorLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_VENDEDOR_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new VendedorDto()
                {
                    #region Campos
                    ve_id = x.Ve_id,
                    ve_nombre = x.Ve_nombre,
                    ve_lista = x.Ve_lista,
                    #endregion
                }).ToList();
        }
    }
}
