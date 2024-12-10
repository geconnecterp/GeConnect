using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class DepartamentoServicio : Servicio<Departamento>, IDepartamentoServicio
    {
        public DepartamentoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<DepartamentoDto> GetDepartamentoPorProvinciaLista(string prov_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PROVINCIA_DEPTOS_LISTA;
            var ps = new List<SqlParameter>() 
            {
                new("@prov_id",prov_id)
            };
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new DepartamentoDto()
                {
                    #region Campos
                    prov_id = x.Prov_id,
                    dep_id = x.Dep_id,
                    dep_lista = x.Dep_lista,
                    dep_nombre = x.Dep_nombre,
                    #endregion
                }).ToList();
        }
    }
}
