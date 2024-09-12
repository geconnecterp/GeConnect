using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Runtime.Intrinsics.Arm;

namespace gc.api.core.Servicios
{
	public class ApiProductoServicio : Servicio<Producto>, IApiProductoServicio
	{
		public ApiProductoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{

		}

		public override PagedList<Producto> GetAll(QueryFilters filters)
		{
			filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
			filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

			var productoss = GetAllIq();
			productoss = productoss.OrderBy($"{filters.Sort} {filters.SortDir}");

			if (!filters.Todo)
			{
				if (filters.Id != null && filters.Id != default)
				{
					productoss = productoss.Where(r => r.P_Id == (string)filters.Id);
				}
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_M_Marca.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_M_Desc.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_M_Capacidad.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Id_Prov.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Desc.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Desc_Ticket.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Up_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Rub_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Cta_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Pg_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Boni.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Usu_Id_Alta.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Usu_Id_Modi.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Obs.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.P_Balanza_Id.Contains(filters.Search));
			}

			if (!string.IsNullOrEmpty(filters.Search))
			{
				productoss = productoss.Where(r => r.Lp_Id_Default.Contains(filters.Search));
			}

			var paginas = PagedList<Producto>.Create(productoss, filters.PageNumber ?? 1, filters.PageSize ?? 20);

			return paginas;
		}

		public List<InfoProdLP> InfoProductoLP(string id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_LP;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@p_id",id),

			};

			List<InfoProdLP> producto = _repository.EjecutarLstSpExt<InfoProdLP>(sp, ps, true);

			return producto;
		}

		public List<InfoProdMovStk> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_MOVSTK;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@p_id",id),
					new SqlParameter("@adm_id",adm),
					new SqlParameter("@depo_id",depo),
					new SqlParameter("@sm_tipo",tmov),
					new SqlParameter("@d",desde),
					new SqlParameter("@h",hasta),
			};

			List<InfoProdMovStk> producto = _repository.EjecutarLstSpExt<InfoProdMovStk>(sp, ps, true);

			return producto;
		}

		public List<InfoProdStkA> InfoProductoStkA(string id, string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_STKA;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@p_id",id),
					new SqlParameter("@adm_id",admId),
			};

			List<InfoProdStkA> producto = _repository.EjecutarLstSpExt<InfoProdStkA>(sp, ps, true);

			return producto;
		}

		public List<InfoProdStkBox> InfoProductoStkBoxes(string id, string adm, string depo)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_STKBOX;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@p_id",id),
					new SqlParameter("@adm_id",adm),
					new SqlParameter("@depo_id",depo),
			};

			List<InfoProdStkBox> producto = _repository.EjecutarLstSpExt<InfoProdStkBox>(sp, ps, true);

			return producto;
		}

		public List<InfoProdStkD> InfoProductoStkD(string id, string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_STKD;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@p_id",id),
					new SqlParameter("@adm_id",admId),
			};

			List<InfoProdStkD> producto = _repository.EjecutarLstSpExt<InfoProdStkD>(sp, ps, true);

			return producto;
		}

		public ProductoBusquedaDto ProductoBuscar(BusquedaBase busqueda)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRODUCTO_BUSQUEDA;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@busqueda",busqueda.Busqueda),
					new SqlParameter("@lp_id",busqueda.ListaPrecio?? ""),
					new SqlParameter("@adm_id",busqueda.Administracion),
					new SqlParameter("co_tipo",busqueda.TipoOperacion),
					new SqlParameter("cli_dto",busqueda.DescuentoCli)
			};

			List<ProductoBusquedaDto> producto = _repository.EjecutarLstSpExt<ProductoBusquedaDto>(sp, ps, true);

			if (producto.Count > 0)
			{
				return producto.First();
			}
			else
			{
				return new();
			}
		}

		public List<ProductoListaDto> ProductoListaBuscar(BusquedaProducto search)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRODUCTO_BUSQUEDA;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@busqueda",search.Busqueda),
				new("@lp_id",search.ListaPrecio??""),
				new("@adm_id",search.Administracion),
				new("@cta_id",search.CtaProveedorId),
				new("@cta_id_unico",search.CtaProveedorIdUnico),
				new("@rub_id",search.RubroId),
				new("@rub_id_unico",search.RubroIdUnico),
				new("@activo",search.EstadoActivo),
				new("@discontinuo",search.EstadoDiscont),
				new("@inactivo",search.EstadoInactivo),
				new("@stk_no",search.SinStock),
				new("@stk_si", search.ConStock)
			};

			List<ProductoListaDto> productos = _repository.EjecutarLstSpExt<ProductoListaDto>(sp, ps, true);

			return productos;
		}

		public List<RPRAutorizacionPendienteDto> RPRObtenerAutorizacionPendiente(string adm)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_PENDIENTES;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",adm),
			};

			List<RPRAutorizacionPendienteDto> productos = _repository.EjecutarLstSpExt<RPRAutorizacionPendienteDto>(sp, ps, true);

			return productos;
		}

		public List<RespuestaDto> RPRCargar(RPRCargarRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_CARGAR;
			var ps = new List<SqlParameter>()
			{
				new("@json",request.json_str),
			};
			List<RespuestaDto> productos = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return productos;
		}

		public List<RespuestaDto> RPRElimina(string rp)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_ELIMINA;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
			};
			List<RespuestaDto> productos = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return productos;
		}

		public List<JsonDto> RPREObtenerDatosJsonDesdeRP(string rp)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_DATOS_JSON;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
			};
			List<JsonDto> jsonstring = _repository.EjecutarLstSpExt<JsonDto>(sp, ps, true);

			return jsonstring;
		}

		public List<RPRItemVerCompteDto> RPRObtenerDatosVerCompte(string rp)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_VER_COMPTES;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
			};
			List<RPRItemVerCompteDto> items = _repository.EjecutarLstSpExt<RPRItemVerCompteDto>(sp, ps, true);

			return items;
		}

		public List<RPRVerConteoDto> RPRObtenerConteos(string rp)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_VER_CONTEOS;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
			};
			List<RPRVerConteoDto> items = _repository.EjecutarLstSpExt<RPRVerConteoDto>(sp, ps, true);

			return items;
		}

		public RPRRegistroResponseDto RPRRegistrarProductos(string json)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_REGISTRA;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@json",json),
			};

			List<RPRRegistroResponseDto> productos = _repository.EjecutarLstSpExt<RPRRegistroResponseDto>(sp, ps, true);

			return productos.First();
		}

		public List<RPRAutoComptesPendientesDto> RPRObtenerComptesPendientes(string adm)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_COMPTES_PENDIENTES;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id", adm)
			};
			List<RPRAutoComptesPendientesDto> comptes_pendientes = _repository.EjecutarLstSpExt<RPRAutoComptesPendientesDto>(sp, ps, true);
			return comptes_pendientes;
		}
	}
}
