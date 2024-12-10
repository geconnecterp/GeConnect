using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class NaturalezaJuridicaServicio : Servicio<NaturalezaJuridica>, INaturalezaJuridicaServicio
    {
        public NaturalezaJuridicaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<NaturalezaJuridicaDto> GetNaturalezaJuridicaLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_NATURALEZA_JURIDICA_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new NaturalezaJuridicaDto()
                {
                    #region Campos
                    nj_id = x.Nj_id,
                    nj_desc = x.Nj_desc,
                    nj_tipo = x.Nj_tipo,
                    nj_lista = x.Nj_lista,
                    #endregion
                }).ToList();
        }
    }
}
