using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios.ABM
{
    public class ABMCuentaDirectaServicio : Servicio<CuentaGasto>, IABMCuentaDirectaServicio
	{
		public ABMCuentaDirectaServicio(IUnitOfWork uow, IOptions<PaginationOptions> pag) : base(uow, pag)
		{
		}

		public List<ABMCuentaDirectaSearchDto> Buscar(QueryFilters filtro)
		{
			filtro.Pagina = filtro.Pagina == null || filtro.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtro.Pagina;
			filtro.Registros = filtro.Registros == null || filtro.Registros <= 0 ? _pagSet.DefaultPageSize : filtro.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_ABM_GASTOS_LISTA;

			var ps = new List<SqlParameter>();

			//debo cargar aca todos los filtro sobre los parametros a utilizar
			if (!string.IsNullOrEmpty(filtro.Id))
			{
				ps.Add(new SqlParameter("@id", true));
				//hay un id de producto. se habilita la seccion de productos
				ps.Add(new SqlParameter("@id_d", filtro.Id));

				if (!string.IsNullOrEmpty(filtro.Id2))
				{
					ps.Add(new SqlParameter("@id_h", filtro.Id2));
				}
				else
				{
					ps.Add(new SqlParameter("@id_h", filtro.Id));
				}
			}
			else
			{
				ps.Add(new SqlParameter("@id", false));
			}

			//se carga si es necesario los parametros del sp
			if (!string.IsNullOrEmpty(filtro.Buscar))
			{
				ps.Add(new SqlParameter("@deno", true));
				ps.Add(new SqlParameter("@deno_like", filtro.Buscar));
			}

			if (filtro.Rel01 != null && filtro.Rel01.Count > 0)
			{
				//ps.Add(new SqlParameter("@ope", true));
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtro.Rel01)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append(',');
					}
					sb.Append(item);
				}
				ps.Add(new SqlParameter("@tipo", true));
				ps.Add(new SqlParameter("@tcg_id", sb.ToString()));
			}
			
			//cantidad de registros
			ps.Add(new SqlParameter("@registros", filtro.Registros));
			//pagina de visualización => Si se filtran producto "Fernet" y se hayan 54 reg.
			//Si los registros de 1 pagina son 200 y la pagina es 1, se presentaran los 54 reg.
			//Si la pagina solicitada es la 2, se devolveran 0 registros.
			ps.Add(new SqlParameter("@pagina", filtro.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtro.Sort ?? ""));

			List<ABMCuentaDirectaSearchDto> mp = _repository.EjecutarLstSpExt<ABMCuentaDirectaSearchDto>(sp, ps, true);

			return mp;
		}
	}
}
