using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Response;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;


namespace gc.api.core.Servicios
{
    public class ApiProductoServicio : Servicio<Producto>, IApiProductoServicio
	{
		public ApiProductoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{

		}

		public override PagedList<Producto> GetAll(QueryFilters filters)
		{
			filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
			filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

			var productoss = GetAllIq();
			productoss = productoss.OrderBy($"{filters.Sort} {filters.SortDir}");

			if (!filters.Todo)
			{
				if (filters.Id != null && filters.Id != default)
				{
					productoss = productoss.Where(r => r.P_Id == (string)filters.Id);
				}
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_M_Marca.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_M_Desc.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_M_Capacidad.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Id_Prov.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Desc.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Desc_Ticket.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Up_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Rub_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Cta_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Pg_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Boni.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Usu_Id_Alta.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Usu_Id_Modi.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Obs.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.P_Balanza_Id.Contains(filters.Buscar));
			}

			if (!string.IsNullOrEmpty(filters.Buscar))
			{
				productoss = productoss.Where(r => r.Lp_Id_Default.Contains(filters.Buscar));
			}

			var paginas = PagedList<Producto>.Create(productoss, filters.Pagina ?? 1, filters.Registros ?? 20);

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

		public List<InfoProdStkBox> InfoProductoStkBoxes(string id, string adm, string depo, string box = "")
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_STKBOX;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@p_id",id),
					new SqlParameter("@adm_id",adm),
					new SqlParameter("@depo_id",depo),
					new SqlParameter("@box_id",box),
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

		public List<NDeCYPI.InfoProdIExMesDto> InfoProdIExMes(string admId, string pId, int meses)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_IE_MESES;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@adm_id",admId),
					new SqlParameter("@p_id",pId),
					new SqlParameter("@meses",meses),
			};

			List<NDeCYPI.InfoProdIExMesDto> producto = _repository.EjecutarLstSpExt<NDeCYPI.InfoProdIExMesDto>(sp, ps, true);

			return producto;
		}

		public List<NDeCYPI.InfoProdIExSemanaDto> InfoProdIExSemana(string admId, string pId, int semanas)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_IE_SEMANAS;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@adm_id",admId),
					new SqlParameter("@p_id",pId),
					new SqlParameter("@semanas",semanas),
			};

			List<NDeCYPI.InfoProdIExSemanaDto> producto = _repository.EjecutarLstSpExt<NDeCYPI.InfoProdIExSemanaDto>(sp, ps, true);

			return producto;
		}

		public List<ProductoNCPISustitutoDto> InfoProdSustituto(string pId, string tipo, string admId, bool soloProv)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_SUSTITUTO;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@p_id",pId),
					new SqlParameter("@tipo",tipo),
					new SqlParameter("@adm_id",admId),
					new SqlParameter("@solo_prov",soloProv),
			};

			List<ProductoNCPISustitutoDto> producto = _repository.EjecutarLstSpExt<ProductoNCPISustitutoDto>(sp, ps, true);

			return producto;
		}

		public List<NDeCYPI.InfoProductoDto> InfoProd(string pId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@p_id",pId),
			};

			List<NDeCYPI.InfoProductoDto> producto = _repository.EjecutarLstSpExt<NDeCYPI.InfoProductoDto>(sp, ps, true);

			return producto;
		}

		public List<TipoAjusteDeStockDto> ObtenerTipoDeAjusteDeStock()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_AJ_TIPOS;

			var ps = new List<SqlParameter>();

			List<TipoAjusteDeStockDto> ajustes = _repository.EjecutarLstSpExt<TipoAjusteDeStockDto>(sp, ps, true);

			return ajustes;
		}

		public List<AjustePrevioCargadoDto> ObtenerAJPreviosCargados(string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_AJ_PREVIOS_CARGADOS;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@adm_id",admId),
			};

			List<AjustePrevioCargadoDto> ajustes = _repository.EjecutarLstSpExt<AjustePrevioCargadoDto>(sp, ps, true);

			return ajustes;
		}

		public List<AjusteRevertidoDto> ObtenerAJREVERTIDO(string ajId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_AJ_AJUSTE_REVERTIDO;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@as_compte",ajId),
			};

			List<AjusteRevertidoDto> ajustes = _repository.EjecutarLstSpExt<AjusteRevertidoDto>(sp, ps, true);

			return ajustes;
		}

		public List<RespuestaDto> ConfirmarAjusteStk(ConfirmarAjusteStkRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_AJ_CONFIRMA;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@json",request.json),
					new SqlParameter("@adm_id",request.admId),
					new SqlParameter("@usu_id",request.usuId),
					new SqlParameter("@compte_ori",request.compteOri==string.Empty?null:request.compteOri),
			};

			List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return respuesta;
		}

		public List<DevolucionPrevioCargadoDto> ObtenerDPPreviosCargados(string admId, string ctaId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_DV_PREVIOS_CARGADOS;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@adm_id",admId),
					new SqlParameter("@cta_id",ctaId),
			};

			List<DevolucionPrevioCargadoDto> ajustes = _repository.EjecutarLstSpExt<DevolucionPrevioCargadoDto>(sp, ps, true);

			return ajustes;
		}

		public List<DevolucionRevertidoDto> ObtenerDPREVERTIDO(string dvCompte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_DV_DATOS;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@dv_compte",dvCompte),
			};

			List<DevolucionRevertidoDto> ajustes = _repository.EjecutarLstSpExt<DevolucionRevertidoDto>(sp, ps, true);

			return ajustes;
		}

		public List<RespuestaDto> ConfirmarDP(ConfirmarDPRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_DV_CONFIRMA;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@json",request.json),
					new SqlParameter("@adm_id",request.admId),
					new SqlParameter("@usu_id",request.usuId),
			};

			List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return respuesta;
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

		public List<ProductoBusquedaDto> ProductoBuscarPorIds(BusquedaBase busqueda)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRODUCTO_BUSQUEDA_MUCHOS;

			var ps = new List<SqlParameter>()
			{
					new SqlParameter("@lista",busqueda.Busqueda),
					new SqlParameter("@lp_id",busqueda.ListaPrecio?? ""),
					new SqlParameter("@adm_id",busqueda.Administracion),
					new SqlParameter("co_tipo",busqueda.TipoOperacion),
					new SqlParameter("cli_dto",busqueda.DescuentoCli)
			};

			List<ProductoBusquedaDto> producto = _repository.EjecutarLstSpExt<ProductoBusquedaDto>(sp, ps, true);

			return producto;
		}

		public List<ProductoListaDto> ProductoListaBuscar(BusquedaProducto search)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRODUCTO_BUSQUEDA_LISTA;
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
				new("@stk_no",!search.SinStock),
				new("@stk_si", !search.ConStock),
				new("@registros", search.Registros),
				new("@pagina", search.Pagina),
                new("@ordenar", search.Sort),

			};

			List<ProductoListaDto> productos = _repository.EjecutarLstSpExt<ProductoListaDto>(sp, ps, true);

			return productos;
		}

		public List<AutorizacionPendienteDto> RPRObtenerAutorizacionPendiente(string adm)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_PENDIENTES;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",adm),
			};

			List<AutorizacionPendienteDto> productos = _repository.EjecutarLstSpExt<AutorizacionPendienteDto>(sp, ps, true);

			return productos;
		}

		public List<RespuestaDto> RPRCargar(CargarJsonGenRequest request)
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

		public List<RespuestaDto> RPRConfirma(string rp, string adm_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_CONFIRMA;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
				new("@adm_id",adm_id),
			};
			List<RespuestaDto> productos = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return productos;
		}

		public List<RPRxULDto> RPRxUL(string rp)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_UL;
			var ps = new List<SqlParameter>()
			{
				new("@rp",rp),
			};
			List<RPRxULDto> rpr_ul = _repository.EjecutarLstSpExt<RPRxULDto>(sp, ps, true);

			return rpr_ul;
		}

		public List<RPRxULDetalleDto> RPRxULDetalle(string ulId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_UL_D;
			var ps = new List<SqlParameter>()
			{
				new("@ul_id",ulId),
			};
			List<RPRxULDetalleDto> rpr_ul = _repository.EjecutarLstSpExt<RPRxULDetalleDto>(sp, ps, true);

			return rpr_ul;
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

		public RegistroResponseDto RPRRegistrarProductos(string json, bool esModificacion)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_REGISTRA;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@json",json),
				new("@ul_modifica",esModificacion)
			};

			List<RegistroResponseDto> productos = _repository.EjecutarLstSpExt<RegistroResponseDto>(sp, ps, true);

			return productos.First();
		}

		public List<AutoComptesPendientesDto> RPRObtenerComptesPendientes(string adm)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_COMPTES_PENDIENTES;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id", adm)
			};
			List<AutoComptesPendientesDto> comptes_pendientes = _repository.EjecutarLstSpExt<AutoComptesPendientesDto>(sp, ps, true);
			return comptes_pendientes;
		}


		public RespuestaDto ValidaProductoCarrito(TiProductoCarritoDto request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Carrito_Valida;
			var ps = new List<SqlParameter>()
			{
				new("@ti", request.Ti),
				new("@adm_id",request.AdmId),
				new("@usu_id",request.UsuId),
				new("@box_id",request.BoxId),
				new("@desarma_box",request.Desarma),
				new("@p_id",request.Pid),
				new("@unidad_pres",request.Unidad_pres),
				new("@bulto",request.Bulto),
				new("@us",request.Us),
				new("@cantidad",request.Cantidad),
				new("@fv",request.Fvto)
			};
			List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}

		public RespuestaDto ValidarProductoCarrito(TiProductoCarritoDto request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Carrito_Valida;
			var ps = new List<SqlParameter>()
			{
				new("@ti", request.Ti),
				new("@adm_id",request.AdmId),
				new("@usu_id",request.UsuId),
				new("@box_id",request.BoxId),
				new("@desarma_box",request.Desarma),
				new("@p_id",request.Pid),
				new("@unidad_pres",request.Unidad_pres),
				new("@bulto",request.Bulto),
				new("@us",request.Us),
				new("@cantidad",request.Cantidad),
				new("@fv",request.Fvto)
			};
			List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}

		public RespuestaDto ResguardarProductoCarrito(TiProductoCarritoDto request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Carrito_Carga;
			var ps = new List<SqlParameter>()
			{
				new("@ti", request.Ti),
				new("@adm_id",request.AdmId),
				new("@usu_id",request.UsuId),
				new("@box_id",request.BoxId),
				new("@desarma_box",request.Desarma),
				new("@p_id",request.Pid),
				new("@unidad_pres",request.Unidad_pres),
				new("@bulto",request.Bulto),
				new("@us",request.Us),
				new("@cantidad",request.Cantidad),
				new("@fv",request.Fvto)
			};
			List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}

		public List<TRPendienteDto> ObtenerTRPendientes(ObtenerTRPendientesRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Pendientes;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",request.admId),
				new("@usu_id",request.usuId),
				new("@tit_id",request.titId),
			};
			List<TRPendienteDto> respuesta = _repository.EjecutarLstSpExt<TRPendienteDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRAutSucursalesDto> ObtenerTRAut_Sucursales(string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_Sucursales;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",admId),
			};
			List<TRAutSucursalesDto> respuesta = _repository.EjecutarLstSpExt<TRAutSucursalesDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRAutPIDto> ObtenerTRAut_PI(string admId, string admIdLista)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_PI;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",admId),
				new("@adm_id_lista",admIdLista),
			};
			List<TRAutPIDto> respuesta = _repository.EjecutarLstSpExt<TRAutPIDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRAutPIDetalleDto> ObtenerTRAut_PI_Detalle(string piCompte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_PI_Detalle;
			var ps = new List<SqlParameter>()
			{
				new("@pi_compte",piCompte),
			};
			List<TRAutPIDetalleDto> respuesta = _repository.EjecutarLstSpExt<TRAutPIDetalleDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRAutDepoDto> ObtenerTRAut_Depositos(string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_Depositos;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",admId),
			};
			List<TRAutDepoDto> respuesta = _repository.EjecutarLstSpExt<TRAutDepoDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRAutAnalizaDto> TRAutAnaliza(string listaPi, string listaDepo, bool stkExistente, bool sustituto, int palletNro)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_Analiza;
			var ps = new List<SqlParameter>()
			{
				new("@lista_pi",listaPi),
				new("@lista_depo",listaDepo),
				new("@stk_existente",stkExistente),
				new("@sustituto",sustituto),
				new("@palet_nro",palletNro),
			};
			List<TRAutAnalizaDto> respuesta = _repository.EjecutarLstSpExt<TRAutAnalizaDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRProductoParaAgregar> TRObtenerSustituto(string pId, string listaDepo, string admIdDes, string tipo)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_sustituto;
			var ps = new List<SqlParameter>()
			{
				new("@p_id",pId),
				new("@lista_depo",listaDepo),
				new("@adm_id_des",admIdDes),
				new("@tipo",tipo),
			};
			List<TRProductoParaAgregar> respuesta = _repository.EjecutarLstSpExt<TRProductoParaAgregar>(sp, ps, true);
			return respuesta;
		}
		public List<RespuestaDto> TRConfirmaAutorizaciones(TRConfirmaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Aut_Nuevas;
			var ps = new List<SqlParameter>()
			{
				new("@json",request.json),
				new("@adm_id",request.admId),
				new("@usu_id",request.usuId),
			};
			List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return respuesta;
		}

		public List<TRVerConteosDto> TRVerConteos(string ti)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Ver_Conteos;
			var ps = new List<SqlParameter>()
			{
				new("@ti",ti),
			};
			List<TRVerConteosDto> respuesta = _repository.EjecutarLstSpExt<TRVerConteosDto>(sp, ps, true);
			return respuesta;
		}
		public List<RespuestaDto> TRValidarTransferencia(TRValidarTransferenciaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Ctl_Salida;
			var ps = new List<SqlParameter>()
			{
				new("@ti",request.ti),
				new("@adm_id",request.admId),
				new("@usu_id",request.usuId),
			};
			List<RespuestaDto> respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return respuesta;
		}
		public List<ProductoNCPIDto> NCPICargarListaDeProductos(NCPICargarListaDeProductosRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OC_Productos;
			var ps = new List<SqlParameter>()
			{
				new("@tipo",request.Tipo),
				new("@adm_id",request.AdmId),
				new("@filtro",request.Filtro),
				new("@id",request.Id),
			};
			List<ProductoNCPIDto> respuesta = _repository.EjecutarLstSpExt<ProductoNCPIDto>(sp, ps, true);
			return respuesta;
		}
		public List<NCPICargaPedidoResponse> NCPICargaPedido(NCPICargaPedidoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OC_Carga_Pedido;
			var ps = new List<SqlParameter>()
			{
				new("@tipo",request.tipo),
				new("@adm_id",request.admId),
				new("@usu_id",request.usuId),
				new("@p_id",request.pId),
				new("@tipo_carga",request.tipoCarga),
				new("@bultos",request.bultos),
			};
			List<NCPICargaPedidoResponse> respuesta = _repository.EjecutarLstSpExt<NCPICargaPedidoResponse>(sp, ps, true);
			return respuesta;
		}
		//}

		/// <summary>
		/// para generar una nueva TI para tr sin autorizacion
		/// </summary>
		/// <param name="ti">(tipo de TR, pasar “E” 'Depósitos Sin Autorización y “O”  Box Sin Autorización)</param>
		/// <param name="adm"></param>
		/// <param name="usu"></param>
		/// <returns></returns>
		public TIRespuestaDto TRNuevaSinAuto(string ti, string adm, string usu)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Nueva_Sin_Au;
			var ps = new List<SqlParameter>()
			{
				new("@tit_d", ti),
				new("@adm_id",adm),
				new("@usu_id",usu),
			};
			List<TIRespuestaDto> resp = _repository.EjecutarLstSpExt<TIRespuestaDto>(sp, ps, true);
			return resp.First();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="usu"></param>
		/// <returns></returns>
		public TIRespuestaDto TRValidaPendiente(string usu)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_VALIDA_PENDIENTE;
			var ps = new List<SqlParameter>()
			{
				new("@usu_id",usu),
			};
			List<TIRespuestaDto> resp = _repository.EjecutarLstSpExt<TIRespuestaDto>(sp, ps, true);
			return resp.First();
		}
		public RespuestaDto TRCtrlSalida(string ti, string adm, string usu)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Control_Salida;
			var ps = new List<SqlParameter>()
			{
				new("@ti", ti),
				new("@adm_id",adm),
				new("@usu_id",usu),
			};
			List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}
		public RespuestaDto TR_Confirma(TIRequestConfirmaDto conf)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_CONFIRMA;
			var ps = new List<SqlParameter>()
			{
				new("@ti", conf.Ti),
				new("@adm_id",conf.AdmId),
				new("@usu_id",conf.Usu),
				new("@box_id",conf.BoxDest),
			};
			List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}

		public List<ProductoGenDto> TRVerCtrlSalida(string tr, string user)
		{

			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_VER_CTRL_SALIDA;
			var ps = new List<SqlParameter>()
			{
				new("@ti",tr),
				new("@usu_id",user),
			};
			List<ProductoGenDto> resp = _repository.EjecutarLstSpExt<ProductoGenDto>(sp, ps, true);
			return resp;
		}

		public RespuestaDto TRCargarCtrlSalida(string json)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_CARGAR_CTRL_SALIDA;
			/// varchar(30),  varchar(3),  varchar(4), varchar(8), @ bit, @ varchar(10) , @ bit, @ bit, @ bit, @ bit, @ bit, @ bit)  
			var ps = new List<SqlParameter>()
			{
				new("@json",json),
			};

			List<RespuestaDto> productos = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

			return productos.First();
		}

		public BoxInfoDto ObtenerBoxInfo(string box_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_BOX_INFO;
			var ps = new List<SqlParameter>()
			{
				new("@box_id",box_id),
			};

			List<BoxInfoDto> info = _repository.EjecutarLstSpExt<BoxInfoDto>(sp, ps, true);

			return info.First();
		}

		public List<ConsULDto> ConsultarUL(string tipo, string admId, DateTime? desde,DateTime? hasta)
		{
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFO_UL_REPO;
			var ps = new List<SqlParameter>();
			ps.Add(new SqlParameter("@tipo", tipo));
            var p = new SqlParameter("@desde", desde)
            {
                IsNullable = true,
                Direction = ParameterDirection.Input,
                SqlDbType = SqlDbType.DateTime,
				SqlValue = desde
            };
            ps.Add(p);

            p = new SqlParameter("@hasta", hasta)
            {
                IsNullable = true,
                Direction = ParameterDirection.Input,
                SqlDbType = SqlDbType.DateTime,
				SqlValue = hasta
            };
            ps.Add(p);

			ps.Add(new SqlParameter("@adm_id", admId));
            

            List<ConsULDto> uls = _repository.EjecutarLstSpExt<ConsULDto>(sp, ps, true);

            return uls;
        }

		public List<BoxInfoStkDto> ObtenerBoxInfoStk(string box_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_BOX_INFO_STK;
			var ps = new List<SqlParameter>()
			{
				new("@box_id",box_id),
			};

			List<BoxInfoStkDto> stks = _repository.EjecutarLstSpExt<BoxInfoStkDto>(sp, ps, true);

			return stks;
		}

		public List<BoxInfoMovStkDto> ObtenerBoxInfoMovStk(string box_id, string sm_tipo, DateTime desde, DateTime hasta)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_BOX_INFO_MOV_STK;
			var ps = new List<SqlParameter>()
			{
				new("@box_id",box_id),
				new("@sm_tipo",sm_tipo),
				new("@d",desde),
				new("@h",hasta),
			};

			List<BoxInfoMovStkDto> movs = _repository.EjecutarLstSpExt<BoxInfoMovStkDto>(sp, ps, true);

			return movs;
		}

        public RespuestaDto AJ_CargaConteosPrevios(string json, string admid)
        {
			var sp = ConstantesGC.StoredProcedures.SP_AJ_CARGA_CONTEOS_PREVIA;
			var ps = new List<SqlParameter>()
			{
				new("@json",json),
				new("@adm_id",admid),
			};

            List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
        }

        public RespuestaDto DV_CargaConteosPrevios(string json, string admid)
        {
            var sp = ConstantesGC.StoredProcedures.SP_DV_CARGA_CONTEOS_PREVIA;
            var ps = new List<SqlParameter>()
            {
                new("@json",json),
                new("@adm_id",admid),
            };

            List<RespuestaDto> resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return resp.First();
        }

		public List<MedidaDto> ObtenerMedidas()
		{
            var sp = ConstantesGC.StoredProcedures.SP_UP_LISTA;
            var ps = new List<SqlParameter>();

            List<MedidaDto> resp = _repository.EjecutarLstSpExt<MedidaDto>(sp, ps, true);
			return resp;
        }

		public List<IVASituacionDto> ObtenerIVASituacion()
		{
            var sp = ConstantesGC.StoredProcedures.SP_IVA_SITUACION_LISTA;
            var ps = new List<SqlParameter>();

            List<IVASituacionDto> resp = _repository.EjecutarLstSpExt<IVASituacionDto>(sp, ps, true);
            return resp;
        }

        public List<IVAAlicuotaDto> ObtenerIVAAlicuotas()
        {
			var sp = ConstantesGC.StoredProcedures.SP_IVA_ALICUOTA_LISTA;
            var ps = new List<SqlParameter>();

            List<IVAAlicuotaDto> resp = _repository.EjecutarLstSpExt<IVAAlicuotaDto>(sp, ps, true);
            return resp;
        }

		public List<ProductoBarradoDto> ObtenerBarradoDeProd(string p_id)
		{
            var sp = ConstantesGC.StoredProcedures.SP_BARRADO_LISTA;
            var ps = new List<SqlParameter>() { new SqlParameter("@p_id",p_id) };

            List<ProductoBarradoDto> resp = _repository.EjecutarLstSpExt<ProductoBarradoDto>(sp, ps, true);
            return resp;
        }

        public List<LimiteStkDto> ObtenerLimiteStk(string p_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_LIMITESTK_LISTA;
            var ps = new List<SqlParameter>() { new SqlParameter("@p_id", p_id) };

            List<LimiteStkDto> resp = _repository.EjecutarLstSpExt<LimiteStkDto>(sp, ps, true);
            return resp;
        }
    }
}
