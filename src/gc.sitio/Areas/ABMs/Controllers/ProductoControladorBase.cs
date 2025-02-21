using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
    [Area("ABMs")]
    public class ProductoControladorBase : ControladorBase
    {
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public ProductoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) :base(options,accessor, logger)
        {
			_setting = options.Value;
			_logger = logger;
		}


        #region ABM
       

        public string DirSortProd
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("DirSortProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return "asc";
                }
                return txt;
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext.Session.SetString("DirSortProd", valor);
            }
        }

        public List<ProductoListaDto> ProductosBuscados
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ProductosBuscados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ProductoListaDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ProductosBuscados", json);
            }
        }

        public MetadataGrid MetadataProd
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("MetadataProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return new MetadataGrid();
                }
                return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
            }
            set
            {
                var valor = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("MetadataProd", valor);
            }

        }

        protected ProductoBarradoDto BarradoSeleccionado
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("BarradoSeleccionado");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<ProductoBarradoDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("BarradoSeleccionado", json);
            }
        }

        public List<ProductoBarradoDto>? ProductoBarrados
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ProductoBarrados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ProductoBarradoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ProductoBarrados", json);
            }
        }

        protected List<LimiteStkDto> LimitesStk
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("LimitesStk");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<LimiteStkDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("LimitesStk", json);
            }
        }

        protected LimiteStkDto LimiteStkSeleccionado
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("LimiteStkSeleccionado");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<LimiteStkDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("LimiteStkSeleccionado", json);
            }
        }

        #endregion

        protected async Task<IActionResult> BusquedaAvanzada(string ri01, string ri02, bool act, bool dis, bool ina,bool cstk, bool sstk, string search, bool buscaNew, IProductoServicio _productoServicio, string sort = "p_id", string sortDir = "asc", int pag = 1)
		{
			List<ProductoListaDto> lista;
			MetadataGrid metadata;
			GridCore<ProductoListaDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (!buscaNew && PaginaGrid == pag)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = ProductosBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ProductosBuscados = lista;
				}
				else
				{
					PaginaGrid = pag;
					//traemos datos desde la base
					var busc = new BusquedaProducto
					{
						Busqueda = search,
						ConStock = cstk,
						SinStock = sstk,
						CtaProveedorId = ri01,
						RubroId = ri02,
						EstadoActivo = act,
						EstadoDiscont = dis,
						EstadoInactivo = ina,
						Registros = _setting.NroRegistrosPagina,
						Pagina = pag,
						Sort = sort,
						SortDir = sortDir,
						Administracion = AdministracionId
					};

					var res = await _productoServicio.BusquedaListaProductos(busc, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataProd = res.Item2 ?? null;
					//metadata = MetadataProd;
					ProductosBuscados = lista;
				}
                metadata = MetadataProd;

                //grillaDatos = GenerarGrilla<ProductoListaDto>(ProductosBuscados, "p_desc");
                grillaDatos = GenerarGrilla<ProductoListaDto>(ProductosBuscados, sort, _setting.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, sortDir);
                return PartialView("_gridProdsAdv", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Avanzada";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda Avanzada");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}


	}
}
