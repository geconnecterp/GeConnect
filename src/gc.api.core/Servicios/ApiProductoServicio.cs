using gc.api.core.Contratos.Servicios;
using Azure.Core;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;
using gc.api.core.Contratos.Servicios;


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
		//}

		/// <summary>
		/// para generar una nueva TI para tr sin autorizacion
		/// </summary>
		/// <param name="ti">(tipo de TR, pasar �E� 'Dep�sitos Sin Autorizaci�n y �O�  Box Sin Autorizaci�n)</param>
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


	}
}
