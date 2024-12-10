using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class FormaDePagoServicio : Servicio<FormaDePago>, IFormaDePagoServicio
    {
        public FormaDePagoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<FormaDePagoDto> GetFormaDePagoLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_FORMA_PAGO_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new FormaDePagoDto()
                {
                    #region Campos
                    fp_id = x.Fp_id,
                    fp_desc = x.Fp_desc,
                    fp_lista = x.Fp_lista,
                    #endregion
                }).ToList();
        }
    }
}
