using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class TipoDocumentoServicio : Servicio<TipoDocumento>, ITiposDocumentoServicio
    {
        public TipoDocumentoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<TipoDocumento> GetAll(QueryFilters filters)
        {
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var tipos_documentoss = GetAllIq();
            tipos_documentoss = tipos_documentoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    tipos_documentoss = tipos_documentoss.Where(r => r.Tdoc_Id == (string)filters.Id);
                }
            }           

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                tipos_documentoss = tipos_documentoss.Where(r => r.Tdoc_Desc.Contains(filters.Buscar));
            }

            var paginas = PagedList<TipoDocumento>.Create(tipos_documentoss, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        public List<TipoDocumentoDto> GetTipoDocumentoLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_DOCUMENTO_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new TipoDocumentoDto()
                {
                    #region Campos
                    Tdoc_Id = x.Tdoc_Id,
                    Tdoc_Desc = x.Tdoc_Desc,
                    Tdoc_Lista = x.Tdoc_Lista,
                    #endregion
                }).ToList();
        }
    }
}
