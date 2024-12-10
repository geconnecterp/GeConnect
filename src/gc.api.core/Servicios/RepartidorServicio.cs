using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class RepartidorServicio : Servicio<Repartidor>, IRepartidorServicio
    {
        public RepartidorServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public List<RepartidorDto> GetRepartidorLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_REPARTIDOR_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new RepartidorDto()
                {
                    #region Campos
                    rp_id = x.Rp_id,
                    rp_nombre = x.Rp_nombre,
                    rp_lista = x.Rp_lista,
                    #endregion
                }).ToList();
        }
    }
}
