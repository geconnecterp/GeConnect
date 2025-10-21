using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Twilio.TwiML.Voice;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ProyeccionDeGastosController : ProyeccionDeGastosControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly IAbmServicio _abmSv;
		public ProyeccionDeGastosController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ProyeccionDeGastosController> logger,
											IFinancieroServicio financieroServicio, IAbmServicio abmSv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_abmSv = abmSv;
		}

		public IActionResult Index()
		{
			var model = new ProyeccionDeGastosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "PROYECCIÓN DE GASTOS";
				ViewData["Titulo"] = titulo;

				var listaProyeccion = _financieroServicio.GetGastosProyLista(TokenCookie);
				ListaProyeccionDeGasto = listaProyeccion;
				model.Fecha = DateTime.Now;
				model.Importe = 0;
				model.Concepto = string.Empty;
				model.GrillaProyeccion = ObtenerGridCoreSmart<ProyeccionDeGastoDto>(MapperProyeccion(listaProyeccion));

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

		public IActionResult CargarProyeccionSeleccionada(int orden, int items)
		{
			var model = new ProyeccionDeGastoSeleccionadaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaProyeccionSeleccionada = ListaProyeccionDeGasto.Where(x => x.items == items).ToList();
				if (listaProyeccionSeleccionada == null || listaProyeccionSeleccionada.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "No se encontró el elemento seleccionado."
					};
					return PartialView("_gridMensaje", response);
				}

				model.FechaProyeccion = listaProyeccionSeleccionada[0].fecha;
				model.ImporteProyeccion = listaProyeccionSeleccionada[0].importe;
				model.ConceptoProyeccion = listaProyeccionSeleccionada[0].concepto;
				model.itemsProyeccion = listaProyeccionSeleccionada[0].items;

				return PartialView("_modalProyeccionDeGastoSeleccionada", model);
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

		public JsonResult AgregarRegistro(DateTime fecha, string concepto, decimal importe)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (string.IsNullOrEmpty(concepto))
					return Json(new { error = true, warn = false, msg = "El concepto no puede estar vacío." });
				if (importe <= 0)
					return Json(new { error = true, warn = false, msg = "El importe debe ser mayor a cero." });
				if (fecha == default)
					return Json(new { error = true, warn = false, msg = "La fecha es inválida." });
				var nuevoGasto = new GastoProyListaDto
				{
					items = 0,
					fecha = fecha,
					concepto = concepto.ToUpper(),
					importe = importe
				};
				var listaProyeccion = ListaProyeccionDeGasto;
				InsertarOrdenadoPorFecha(listaProyeccion, nuevoGasto);
				ListaProyeccionDeGasto = listaProyeccion;
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public IActionResult ActualizarListaDeProyeccionDeGastos()
		{
			var model = new ProyeccionDeGastosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaProyeccion = ObtenerGridCoreSmart<ProyeccionDeGastoDto>(MapperProyeccion(ListaProyeccionDeGasto));
				return PartialView("_grillaProyeccionDeGastos", model);
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
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				///Recargo la lista desde la base de datos
				var listaProyeccion = _financieroServicio.GetGastosProyLista(TokenCookie);
				ListaProyeccionDeGasto = listaProyeccion;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public JsonResult ModificarItemProyeccionDeGasto(int items, DateTime fecha, string concepto, decimal importe)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				///Recargo la lista desde la base de datos
				var listaProyeccion = ListaProyeccionDeGasto;
				var item = listaProyeccion.Where(x => x.items == items);
				if (item == null || item.Count() <= 0)
					return Json(new { error = true, warn = false, msg = "No se encontró el registro para modificar." });

				var itemTemp = item.ToList().First();
				itemTemp.fecha = fecha;
				itemTemp.concepto = concepto;
				itemTemp.importe = importe;

				ListaProyeccionDeGasto = listaProyeccion;
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		[HttpPost]
		public async Task<JsonResult> ConfirmarProyeccionDeGasto()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				AbmGenDto abm = new()
				{
					Json = JsonConvert.SerializeObject(ListaProyeccionDeGasto),
					Objeto = "proy_gastos",
					Administracion = AdministracionId,
					Usuario = UserName,
					Abm = 'A'
				};

				var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
				if (res.Ok)
					return Json(new { error = false, warn = false, msg = "La Proyección de Gastos de ha registrado con éxito." });
				else
				{
					if (res.Entidad != null)
						return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj });
					else
						return Json(new { error = false, warn = false, msg = "La Proyección de Gastos de ha registrado con éxito." });
				}
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public JsonResult EliminarItemProyeccionDeGastos(int items)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				var listaProyeccion = ListaProyeccionDeGasto;
				var item = listaProyeccion.Where(x => x.items == items);
				if (item == null || item.Count() <= 0)
					return Json(new { error = true, warn = false, msg = "No se encontró el registro para eliminar." });

				listaProyeccion = [.. listaProyeccion.Where(x => x.items != items)];
				ListaProyeccionDeGasto = listaProyeccion;
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}
		#region Métodos Privados
		private void InsertarOrdenadoPorFecha(List<GastoProyListaDto> lista, GastoProyListaDto nuevo)
		{
			if (lista == null) throw new ArgumentNullException(nameof(lista));
			if (nuevo == null) throw new ArgumentNullException(nameof(nuevo));

			// Buscar la posición donde insertar usando búsqueda binaria para eficiencia
			int index = lista.BinarySearch(nuevo, Comparer<GastoProyListaDto>.Create((a, b) => a.fecha.CompareTo(b.fecha)));

			if (index < 0)
				index = ~index; // Si no se encuentra, BinarySearch devuelve el complemento bitwise del índice de inserción

			lista.Insert(index, nuevo);
		}

		private List<ProyeccionDeGastoDto> MapperProyeccion(List<GastoProyListaDto> lista)
		{
			if (lista == null || lista.Count == 0)
				return [];
			var resultado = new List<ProyeccionDeGastoDto>();
			int orden = 1;
			decimal acumulado = 0;
			foreach (var item in lista)
			{
				acumulado += item.importe;
				var dto = new ProyeccionDeGastoDto
				{
					orden = orden,
					items = item.items,
					fecha = item.fecha,
					concepto = item.concepto,
					importe = item.importe,
					acumulado = acumulado
				};
				resultado.Add(dto);
				orden++;
			}
			return resultado;
		}
		#endregion
	}
}
