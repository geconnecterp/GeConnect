using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class ProvinciaServicio : Servicio<Provincia>, IProvinciaServicio
    {
        public ProvinciaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<ProvinciaDto> GetProvinciaLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PROVINCIA_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new ProvinciaDto()
                {
                    #region Campos
                    prov_id = x.Prov_id,
                    prov_nombre = x.Prov_nombre,
                    prov_lista = x.Prov_lista,
                    #endregion
                }).ToList();
        }
    }
}
