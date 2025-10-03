using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class CargarExtractoBancarioController : CargarExtractoBancarioControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoConciliadoServicio _tipoConciliadoServicio;
		private readonly string tipoCTAF = "BA";
		public CargarExtractoBancarioController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<CargarExtractoBancarioController> logger,
												  IDocManagerServicio docManager, IOptions<DocsManager> docsManager, ITipoConciliadoServicio tipoConciliadoServicio,
												  IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_tipoConciliadoServicio = tipoConciliadoServicio;
		}
		public IActionResult Index()
		{
			var model = new FiltroExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGAR EXTRACTO BANCARIO";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(model);

				return View(model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public JsonResult ObtenerCuentaBanco(string ctaf_id)
		{
			try
			{
				if (ctaf_id == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var lista = ListaCuentaBancos.Where(x => x.ctaf_id == ctaf_id);
				if (lista == null || !lista.Any())
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado la cuenta banco solicitada." });

				return Json(new { error = false, warn = false, msg = "", lista.First().ext_fecha });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener los datos de la cuenta banco seleccionada." });
			}
		}

		public IActionResult CargarExtractoBancarioCrud(FinancieroBcoExtractoRequest request)
		{
			var model = new CrudExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}
				var lista = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id);
				model.CuentaBanco = $"{lista.First().ctaf_denominacion} ({lista.First().ctaf_id})";
				model.GrillaExtracto = ObtenerGridCoreSmart<CrudExtractoBancarioDto>(new List<CrudExtractoBancarioDto>());
				return PartialView("_crudExtractoBancario", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult AbrirModalAgregarItemExtracto(string abm, int orden)
		{
			var model = new AgregarItemExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (abm == "A")
				{
					model.Fecha = DateTime.Now;
					model.Comprobante = string.Empty;
					model.Insertar = false;
					model.Debe = 0;
					model.Haber = 0;
					model.Movimiento = ComboTiposConciliado();
					model.abm = abm;
					model.orden = orden;
				}
				else 
				{
					var item = ListaCrudExtractoBancario.Where(x => x.orden == orden).FirstOrDefault();
					if (item == null)
					{
						RespuestaGenerica<EntidadBase> response = new()
						{
							Ok = false,
							EsError = true,
							EsWarn = false,
							Mensaje = "No se ha encontrado el elemento seleccionado."
						};
						return PartialView("_gridMensaje", response);
					}
					else
					{
						model.Fecha = item.ext_fecha;
						model.Comprobante = item.ext_concepto;
						model.abm = abm;
						model.selected = item.extr_id;
						model.Haber = item.ext_haber;
						model.Debe = item.ext_debe;
						model.Insertar = false;
						model.Movimiento = ComboTiposConciliado();
						model.orden = orden;
					}
				}

				return PartialView("_modal_agregar_item_extr", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public JsonResult QuitarItemExtracto(int orden) 
		{
			try
			{
				if (orden <= 0)
					return Json(new { error = true, warn = false, msg = $"Debe especificar un ítem extracto a quitar." });

				var item = ListaCrudExtractoBancario.Where(x=>x.orden==orden).FirstOrDefault();
				if (item ==null)
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado el elemento a quitar." });

				var listaTemp = ListaCrudExtractoBancario;
				listaTemp.Remove(item);
				Reordenar(listaTemp);
				ListaCrudExtractoBancario = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}

		public JsonResult AgregarItemExtracto(ExtractoCrudItemRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var maxOrden = 0;

				var listaTemp = ListaCrudExtractoBancario;
				maxOrden = listaTemp.Any() ? listaTemp.Max(x => x.orden) : 0;
				
				var newCrudItem = new CrudExtractoBancarioDto
				{
					ctaf_id = request.ctaf_id,
					ext_fecha = request.ext_fecha,
					ext_haber = request.ext_haber,
					ext_debe = request.ext_debe,
					extr_id = request.extr_id,
					extr_desc = request.extr_desc,
					ext_concepto = request.ext_concepto,
					abm = request.abm,
					orden = 0
				};

				if (!request.insertar)
				{
					newCrudItem.orden = maxOrden + 1;
					listaTemp.Add(newCrudItem);
				}
				else
				{
					InsertarYReordenar(listaTemp, newCrudItem, request.orden);
				}
				
				ListaCrudExtractoBancario = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}

		public JsonResult ModificarItemExtracto(ExtractoCrudItemRequest request) 
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var listaTemp = ListaCrudExtractoBancario;
				var newCrudItem = listaTemp.Where(x => x.orden == request.orden).FirstOrDefault();
				if (newCrudItem == null)
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado el ítem a modificar." });

				newCrudItem.extr_id = request.extr_id;
				newCrudItem.extr_desc = request.extr_desc;
				newCrudItem.ext_concepto = request.ext_concepto;
				newCrudItem.ext_debe = request.ext_debe;
				newCrudItem.ext_haber = request.ext_haber;

				ListaCrudExtractoBancario = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}

		public IActionResult ObtenerListaExtractoBancario()
		{
			var model = new CrudExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaExtracto = ObtenerGridCoreSmart<CrudExtractoBancarioDto>(ListaCrudExtractoBancario);
				return PartialView("_grillaExtractoBancario", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaCrudExtractoBancario = [];
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		#region Metodos Privados
		public void InsertarYReordenar(List<CrudExtractoBancarioDto> lista, CrudExtractoBancarioDto nuevoItem, int posicion)
		{
			// Validar límites
			if (posicion < 0) posicion = 0;
			if (posicion > lista.Count) posicion = lista.Count;

			// Insertar en la posición deseada
			lista.Insert(posicion, nuevoItem);

			// Reordenar todos los elementos
			for (int i = 0; i < lista.Count; i++)
			{
				lista[i].orden = i + 1; // o i si querés que empiece en 0
			}
		}

		public void Reordenar(List<CrudExtractoBancarioDto> lista)
		{
			// Reordenar todos los elementos
			for (int i = 0; i < lista.Count; i++)
			{
				lista[i].orden = i + 1; // o i si querés que empiece en 0
			}
		}

		private void CargarDatosIniciales(FiltroExtractoModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			ListaCuentaBancos = ctfLista;
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();

			if (TipoConciliadoLista.Count == 0)
				ObtenerTiposConciliado(_tipoConciliadoServicio);
		}
		#endregion
	}
}
