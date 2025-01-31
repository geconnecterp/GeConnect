using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios.ABM
{
	public class ABMProductoServicio : Servicio<Producto>, IABMProductoServicio
	{

		public ABMProductoServicio(IUnitOfWork uow, IOptions<PaginationOptions> pag) : base(uow, pag)
		{
		}

		public ProductoDto Buscar(string p_id)
		{
			if (string.IsNullOrEmpty(p_id) || string.IsNullOrWhiteSpace(p_id))
			{
				throw new NegocioException("No se recepcionó el identificador del producto buscado");
			}

			string sp = ConstantesGC.StoredProcedures.SP_ABM_P_DATOS;
			var ps = new List<SqlParameter> { new SqlParameter("@p_id", p_id) };

			List<ProductoDto> producto = _repository.EjecutarLstSpExt<ProductoDto>(sp, ps, true);
			if (producto.Count > 0)
			{
				return producto.First();
			}
			else
			{
				throw new NegocioException($"No se encontró el Producto buscado ({p_id})");
			}
		}

		public List<ProductoListaDto> Buscar(QueryFilters filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_ABM_P_LISTA;

			var ps = new List<SqlParameter>();

			//debo cargar aca todos los filtros sobre los parametros a utilizar
			if (!string.IsNullOrEmpty(filtros.Id))
			{
				ps.Add(new SqlParameter("@id", true));
				//hay un id de producto. se habilita la seccion de productos
				ps.Add(new SqlParameter("@p_id_d", filtros.Id));

				if (!string.IsNullOrEmpty(filtros.Id2))
				{
					ps.Add(new SqlParameter("@p_id_h", filtros.Id2));
				}
				else
				{
					ps.Add(new SqlParameter("@p_id_h", filtros.Id));
				}
			}
			else
			{
				ps.Add(new SqlParameter("@id", false));
			}

			//se carga si es necesario los parametros del sp
			if (!string.IsNullOrEmpty(filtros.Buscar))
			{
				ps.Add(new SqlParameter("@des", true));
				ps.Add(new SqlParameter("@p_desc_like", filtros.Buscar));
			}

			if (filtros.Rel01 != null && filtros.Rel01.Count > 0)
			{
				ps.Add(new SqlParameter("@prov", true));
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (var item in filtros.Rel01)
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

				ps.Add(new SqlParameter("@prov_list", sb.ToString() + ','));
			}
			else
			{
				ps.Add(new SqlParameter("@prov", false));
			}

			if (filtros.Rel02 != null && filtros.Rel02.Count > 0)
			{
				ps.Add(new SqlParameter("@rub", true));
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (var item in filtros.Rel02)
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

				ps.Add(new SqlParameter("@rub_list", sb.ToString()));
			}
			else
			{
				ps.Add(new SqlParameter("@rub", false));
			}

			if (filtros.Rel03 != null && filtros.Rel03.Count > 0)
			{
				ps.Add(new SqlParameter("@pg", true));
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (var item in filtros.Rel03)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append(',');
					}
					sb.Append(item.Id);
				}

				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
			{
				ps.Add(new SqlParameter("@pg", false));
			}
			//cantidad de registros
			ps.Add(new SqlParameter("@registros", filtros.Registros));
			//pagina de visualización => Si se filtran producto "Fernet" y se hayan 54 reg.
			//Si los registros de 1 pagina son 200 y la pagina es 1, se presentaran los 54 reg.
			//Si la pagina solicitada es la 2, se devolveran 0 registros.
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<ProductoListaDto> producto = _repository.EjecutarLstSpExt<ProductoListaDto>(sp, ps, true);

			//var items = PagedList<ABMProductoSearchDto>.Create(producto, filtros.Pagina.Value, filtros.Registros.Value);

			return producto;
		}
	}
}
