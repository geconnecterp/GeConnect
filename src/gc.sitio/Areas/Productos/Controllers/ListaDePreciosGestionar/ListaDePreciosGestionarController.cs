using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Productos.Models.ListaDePreciosGestionar;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers.ListaDePreciosGestionar
{
	[Area("Productos")]
	public class ListaDePreciosGestionarController : ListaDePreciosGestionarControladorBase
	{
		private readonly IPrecioListaServicio _precioListaSrv;
		private readonly IListaDePrecioServicio _listaPrcSrv;
		private readonly ISectorServicio _sectorServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private const string lp_principal = "001";
		public ListaDePreciosGestionarController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ListaDePreciosGestionarController> logger,
												 IPrecioListaServicio precioListaSrv, IListaDePrecioServicio listaPrcSrv, ISectorServicio sectorServicio,
												 IRubroServicio rubroServicio, ICuentaServicio cuentaServicio) : base(options, contexto, logger)
		{
			_precioListaSrv = precioListaSrv;
			_listaPrcSrv = listaPrcSrv;
			_sectorServicio = sectorServicio;
			_rubroServicio = rubroServicio;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			var model = new ListaDePreciosGestionarModel();
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var titulo = "LISTA DE PRECIOS";
				ViewData["Titulo"] = titulo;

				ListaPrecioRubCta = [];
				var lp = _listaPrcSrv.GetListaPrecio(TokenCookie);
				model.GrillaListaDePrecios = ObtenerGridCoreSmart<ListaPrecioDto>(lp);
				ListaPrecio = lp;
				return View(model);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la vista de Gestión de Lista de Precios");
				TempData["error"] = "Hubo un problema al cargar la vista de Gestión de Lista de Precios. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public IActionResult CargarDatosDeListaDePrecio(string lp_id)
		{
			var model = new ListaPrecioDto();
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (ListaPrecio == null || !ListaPrecio.Any())
					return PartialView("_gridMensaje", CrearRespuestaWarning("No se encontraron listas de precios en la sesión. Por favor, recargue la página."));

				var item = ListaPrecio.FirstOrDefault(x => x.lp_id == lp_id);
				if (item == null)
					return PartialView("_gridMensaje", CrearRespuestaWarning($"No se encontró la lista de precios con ID '{lp_id}' en la sesión."));
				return PartialView("_partialMargenLP", item);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la sección de datos de la lista de precios.");
				TempData["error"] = "Hubo un problema al cargar la sección de datos de la lista de precios.. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public IActionResult CargarDatosDeListaDePrecioRubCta(string lp_id)
		{
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (string.IsNullOrEmpty(lp_id))
					return PartialView("_gridMensaje", CrearRespuestaWarning("Debe especificar el ID de la lista de precios."));

				var lista = new List<ListaPrecioRubCtaDto>();
				if (ListaPrecioRubCta == null || ListaPrecioRubCta.Count == 0)
					lista = _precioListaSrv.ObtenerListaPreciosRubCta(lp_id, TokenCookie).Result.ListaEntidad;
				else
					lista = ListaPrecioRubCta;

				if (lista == null)
					return PartialView("_gridMensaje", CrearRespuestaWarning($"No se encontró la lista de precios para Rub/Cta con ID '{lp_id}' en la sesión."));
				var listaPRubCta = ObtenerGridCoreSmart<ListaPrecioRubCtaDto>(lista);
				ListaPrecioRubCta = lista;
				return PartialView("_partialLPRubrosProv", listaPRubCta);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la sección de datos de la lista de precios para Rub/Cta especificado.");
				TempData["error"] = "Hubo un problema al cargar la sección de datos de la lista de precios para Rub/Cta especificado. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public IActionResult CargarDatosDeSeccionRubCta()
		{
			var model = new MargenRubrosProvModel();
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var listaRubros = _rubroServicio.ObtenerListaRubros("%", TokenCookie);
				model.ListaRubros = ObtenerListaRubros(listaRubros ?? []);
				var listaSectores = _sectorServicio.GetSectoresLista(TokenCookie);
				model.ListaSectores = ObtenerListaSectores(listaSectores ?? []);
				model.Mgn = 0;
				model.CargarPorSector = true;
				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
				if (ProveedoresLista.Count == 0)
					ObtenerProveedores(_cuentaServicio, "%");
				return PartialView("_partialMargenRubrosProv", model);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la sección de datos de la lista de precios para Rub/Cta especificado.");
				TempData["error"] = "Hubo un problema al cargar la sección de datos de la lista de precios para Rub/Cta especificado. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public JsonResult AgregarRegistros(string lpId, string valorSeleccionado, bool porSectores, string ctaId, decimal mgn)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(lpId) || string.IsNullOrEmpty(valorSeleccionado) || string.IsNullOrEmpty(ctaId) || mgn <= 0)
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios." });

				var rubros = new List<RubroListaABMDto>();
				rubros = porSectores
					? _sectorServicio.GetRubroParaABM(valorSeleccionado, TokenCookie)
					: Mapper(_rubroServicio.ObtenerUnRubro(valorSeleccionado, TokenCookie));

				if (rubros == null || rubros.Count == 0)
					return Json(new { error = true, ok = false, mensaje = "No se encontraron rubros para los criterios proporcionados." });

				// VALIDACIÓN PREVIA
				foreach (var r in rubros)
				{
					// Caso 1: el usuario quiere agregar AZUC / %
					if (ctaId == "%")
					{
						bool existe = ListaPrecioRubCta.Any(x =>
							x.rub_id == r.Rub_Id); // existe para cualquier proveedor, incluso %

						if (existe)
						{
							return Json(new
							{
								error = true,
								ok = false,
								mensaje = $"El rubro {r.Rub_Id} ya existe para algún proveedor y no puede agregarse como '%'."
							});
						}
					}
					else
					{
						// Caso 2: el usuario quiere agregar AZUC / C0018526
						bool existe = ListaPrecioRubCta.Any(x =>
							x.rub_id == r.Rub_Id &&
							(x.cta_id == "%" || x.cta_id == ctaId));

						if (existe)
						{
							return Json(new
							{
								error = true,
								ok = false,
								mensaje = $"El rubro {r.Rub_Id} ya existe para el proveedor '{ctaId}' o como '%'."
							});
						}
					}
				}

				var listaRubros = rubros.Select(r => new ListaPrecioRubCtaDto
				{
					lp_id = lpId,
					rub_id = r.Rub_Id,
					rub_desc = r.Rub_Desc,
					cta_id = ctaId,
					cta_denominacion = ProveedoresLista.Where(x => x.Cta_Id == ctaId).FirstOrDefault().Cta_Denominacion,
					lpp_mgn_principal_porc = mgn
				});
				var listaTemp = ListaPrecioRubCta;
				listaTemp.AddRange(listaRubros);
				ListaPrecioRubCta = listaTemp;
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
		}

		public JsonResult RegistrarModificacionesEnListaDePrecios(RegistrarModificacionesEnListaDePreciosRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });

				// VALIDACIÓN DE REQUEST
				var errores = ValidarRequestRegistrarLP(request);

				if (errores.Any())
				{
					return Json(new
					{
						ok = false,
						error = true,
						mensaje = string.Join(" | ", errores)
					});
				}
				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				request.jsonRubCta = ObtenerJson(ListaPrecioRubCta);
				var respuesta = _precioListaSrv.RegistrarModificacionesEnListaDePrecios(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "Las modificaciones se registraron con éxito.");
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
		}
		#region Metodos Privados
		private string ObtenerJson(List<ListaPrecioRubCtaDto> lista)
		{
			if (lista == null || lista.Count == 0)
				return "[]";
			var listaSerializada = lista.Select(x => new
			{
				rub_id = x.rub_id,
				cta_id = x.cta_id,
				lpp_mgn_principal_porc = x.lpp_mgn_principal_porc
			}).ToList();
			var json = JsonConvert.SerializeObject(listaSerializada);
			return json;
		}
		private SelectList ObtenerListaRubros(List<RubroListaDto> rub)
		{
			var lista = rub.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaSectores(List<SectorDto> sectores)
		{
			var lista = sectores.Select(x => new ComboGenDto { Id = x.Sec_Id, Descripcion = x.Sec_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private List<RubroListaABMDto> Mapper(List<RubroItemListaDto> lista)
		{
			if (lista == null || lista.Count <= 0)
				return [];
			var listaMapeada = new List<RubroListaABMDto>();

			lista.ForEach(x =>
			{
				listaMapeada.Add(new RubroListaABMDto
				{
					Rub_Id = x.rub_id,
					Rub_Desc = x.rub_desc,
					Rubg_Id = x.rubg_id,
					Rubg_Desc = x.rubg_desc,
					Sec_Id = x.sec_id,
					Sec_Desc = x.sec_desc,
					Rub_Lista = x.rub_lista,
					Rubg_Lista = x.rubg_lista
				});
			});

			return listaMapeada;
		}

		private List<string> ValidarRequestRegistrarLP(RegistrarModificacionesEnListaDePreciosRequest req)
		{
			var errores = new List<string>();

			// Validación de strings obligatorios
			if (string.IsNullOrWhiteSpace(req.abm))
				errores.Add("El campo 'abm' es obligatorio.");

			if (string.IsNullOrWhiteSpace(req.lpId))
				errores.Add("El campo 'lpId' es obligatorio.");

			if (string.IsNullOrWhiteSpace(req.lpMgnPrincipal))
				errores.Add("El campo 'lpMgnPrincipal' es obligatorio.");

			// Validación de decimales >= 0
			if (req.lpMargen < 0)
				errores.Add("El margen de lista debe ser mayor o igual a 0.");

			if (req.lpMgnPrincipalPorc < 0)
				errores.Add("El margen principal debe ser mayor o igual a 0.");

			if (req.lpPrevisionTot < 0)
				errores.Add("La previsión total debe ser mayor o igual a 0.");

			if (req.lpPrevisionPin < 0)
				errores.Add("La previsión PIN debe ser mayor o igual a 0.");

			return errores;
		}

		#endregion
	}
}
