using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using System.Diagnostics;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDePagoAProveedorController : OrdenDePagoAProveedorControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;

		private const string tabla_obligaciones = "tbListaObligaciones";
		private const string tabla_creditos = "tbListaCreditos";
		private const string tabla_obligaciones_nuevos = "tbListaObligacionesParaAgregar";
		private const string tabla_creditos_nuevos = "tbListaCreditosParaAgregar";

		public OrdenDePagoAProveedorController(IOrdenDePagoServicio ordenDePagoServicio, ICuentaServicio cuentaServicio, ITipoGastoServicio tipoGastoServicio,
											   IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDePagoAProveedorController> logger) : base(options, contexto, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
			_tipoGastoServicio = tipoGastoServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "ORDEN DE PAGO A PROVEEDORES";
				CargarDatosIniciales(true);
				return View();
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

		public IActionResult ValidarProveedor(string cta_id)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var res = _ordenDePagoServicio.GetOPValidacionesPrev(cta_id, TokenCookie);
				OPValidacionPrevLista = res;
				var model = ObtenerGridCoreSmart<OPValidacionPrevDto>(res);
				return PartialView("_grillaValidacionesPrev", model);
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

		[HttpPost]
		public JsonResult CancelarDesdeGrillaDeValidacionesPrevias()
		{
			try
			{
				OPValidacionPrevLista = [];
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar Cancelar la Validación previa" });
			}
		}

		[HttpPost]
		public IActionResult CargarVistaObligacionesYCreditos(string cta_id)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var model = new CargarObligacionesOCreditosModel
				{
					GrillaCreditosNueva = new GridCoreSmart<OPDebitoYCreditoDelProveedorDto>(),
					GrillaObligacionesNuevas = new GridCoreSmart<OPDebitoYCreditoDelProveedorDto>(),
					ctaDir = "",
					listaCtaDir = ComboTipoGasto()
				};
				CtaIdSelected = cta_id;
				OPDebitoNuevaLista = [];
				OPCreditoNuevaLista = [];
				return PartialView("_vistaObligacionesYCreditos", model);
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

		[HttpPost]
		public IActionResult CargarObligacionesOCreditos(char tipo)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var datos = _ordenDePagoServicio.GetOPDebitoYCreditoDelProveedor(CtaIdSelected, tipo, false, AdministracionId, UserName, TokenCookie);
				var model = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(datos);
				if (tipo == 'D')
				{
					OPDebitoOriginalLista = datos;
					OPDebitoLista = datos;
					return PartialView("_grillaObligaciones", model);
				}
				else
				{
					OPCreditoOriginalLista = datos;
					OPCreditoLista = datos;
					return PartialView("_grillaCreditos", model);
				}
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

		private List<OPDebitoYCreditoDelProveedorDto> ObtenerData(char tipo)
		{
			return _ordenDePagoServicio.GetOPDebitoYCreditoDelProveedor(CtaIdSelected, tipo, false, AdministracionId, UserName, TokenCookie);
		}

		[HttpPost]
		public IActionResult CargarOSacarObligacionesOCreditos(CargarOSacarObligacionesOCreditosRequest r)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				ViewBag.MsgErrorEnCargarOSacarObligacionesOCreditos = "";
				if (r.origen.Equals(tabla_obligaciones)) //Carga desde la tabla de Obligaciones
				{
					var model = new CargarNuevasObligacionesModel();
					//Busco el elemento en la lista original de obligaciones
					if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
						OPDebitoOriginalLista = ObtenerData('D');
					
					var item = OPDebitoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						r.cv_importe = item.cv_importe;
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de obligaciones
								var listaTemp = OPDebitoLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								Console.WriteLine(DateTime.Now.ToString());
								//TODO MARCE: ver si conviene meter un delay para evitar el problema de la actualizacion de las listas en sesion.
								Thread.Sleep(TimeSpan.FromMilliseconds(100));
								Console.WriteLine(DateTime.Now.ToString());
								OPDebitoLista = listaTemp;

								//Lo agrego a la lista que uso para cargar la grilla de obligaciones nuevas
								OPDebitoNuevaLista.Add(item);
								model.MsgErrorEnCargarOSacarObligaciones = "";
							}
							else
								model.MsgErrorEnCargarOSacarObligaciones = respuesta.Entidad.resultado_msj;
						}
						else
							model.MsgErrorEnCargarOSacarObligaciones = "Se ha producido un error al intentar generar la transacción de Obligaciones.";
					}
					else
						model.MsgErrorEnCargarOSacarObligaciones = "Se ha producido un error al intentar generar la transacción de Obligaciones.";

					model.GrillaObligacionesNuevas = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoNuevaLista);
					return PartialView("_grillaNuevosObligaciones", model);
				}
				else if (r.origen.Equals(tabla_creditos)) //Carga desde la tabla de Créditos
				{
					var model = new CargarNuevosCreditosModel();
					if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
						OPDebitoOriginalLista = ObtenerData('C');

					//Busco el elemento en la lista original de creditos
					var item = OPCreditoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						r.cv_importe = item.cv_importe;
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de creditos
								var listaTemp = OPCreditoLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								Console.WriteLine(DateTime.Now.ToString());
								Thread.Sleep(TimeSpan.FromMilliseconds(100));
								Console.WriteLine(DateTime.Now.ToString());
								//TODO MARCE: ver si conviene meter un delay para evitar el problema de la actualizacion de las listas en sesion.
								OPCreditoLista = listaTemp;
								//Lo agrego a la lista que uso para cargar la grilla de creditos nuevas
								OPCreditoNuevaLista.Add(item);
								model.MsgErrorEnCargarOSacarCreditos = "";
							}
							else
							{
								model.MsgErrorEnCargarOSacarCreditos = respuesta.Entidad.resultado_msj;
							}
						}
						else
							model.MsgErrorEnCargarOSacarCreditos = "Se ha producido un error al intentar generar la transacción de Créditos.";
					}
					else
						model.MsgErrorEnCargarOSacarCreditos = "Se ha producido un error al intentar generar la transacción de Obligaciones.";

					model.GrillaCreditosNueva = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoNuevaLista);
					return PartialView("_grillaNuevosCreditos", model);
				}
				else if (r.origen.Equals(tabla_obligaciones_nuevos)) //Quitar desde la tabla de Obligaciones Nuevas
				{
					var model = new CargarNuevasObligacionesModel();
					if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
						OPDebitoOriginalLista = ObtenerData('D');

					//Busco el elemento en la lista original de obligaciones
					var item = OPDebitoNuevaLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						//TODO MARCE: Consultar con CR si debo llamar al SP tambien cuando saco elementos de las grillas de Debito/Credito nuevas
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de obligaciones nuevas
								var listaTemp = OPDebitoNuevaLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								//TODO MARCE: ver si conviene meter un delay para evitar el problema de la actualizacion de las listas en sesion.
								Console.WriteLine(DateTime.Now.ToString());
								Thread.Sleep(TimeSpan.FromMilliseconds(100));
								Console.WriteLine(DateTime.Now.ToString());
								OPDebitoNuevaLista = listaTemp;

								//Lo agrego a la lista de olbigaciones inferior, para eso lo busco en la lista original (Backup)
								item = null;
								item = OPDebitoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
								if (item != null)
								{
									model.MsgErrorEnCargarOSacarObligaciones = "";
									var lista = OPDebitoLista;
									Console.WriteLine(DateTime.Now.ToString());
									Thread.Sleep(TimeSpan.FromMilliseconds(100));
									Console.WriteLine(DateTime.Now.ToString());
									lista.Add(item);
									OPDebitoLista = lista;
								}
								else
									model.MsgErrorEnCargarOSacarObligaciones = "Se ha producido un error al intentar restaurar la lista de Obligaciones.";
							}
							else
								model.MsgErrorEnCargarOSacarObligaciones = respuesta.Entidad.resultado_msj;
						}
						else
							model.MsgErrorEnCargarOSacarObligaciones = "Se ha producido un error al intentar generar la transacción de Obligaciones.";
					}
					else
						model.MsgErrorEnCargarOSacarObligaciones = "Se ha producido un error al intentar generar la transacción de Obligaciones.";

					model.GrillaObligacionesNuevas = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoNuevaLista);
					return PartialView("_grillaNuevosObligaciones", model);
				}
				else //Quitar desde la tabla de Créditos nuevos
				{
					var model = new CargarNuevosCreditosModel();
					if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
						OPDebitoOriginalLista = ObtenerData('C');

					//Busco el elemento en la lista original de obligaciones
					var item = OPCreditoNuevaLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						//TODO MARCE: Consultar con CR si debo llamar al SP tambien cuando saco elementos de las grillas de Debito/Credito nuevas
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de obligaciones nuevas
								var listaTemp = OPCreditoNuevaLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								//TODO MARCE: ver si conviene meter un delay para evitar el problema de la actualizacion de las listas en sesion.
								Console.WriteLine(DateTime.Now.ToString());
								Thread.Sleep(TimeSpan.FromMilliseconds(100));
								Console.WriteLine(DateTime.Now.ToString());
								OPCreditoNuevaLista = listaTemp;

								//Lo agrego a la lista de creditos inferior, para eso lo busco en la lista original (Backup)
								item = null;
								item = OPCreditoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
								if (item != null)
								{
									model.MsgErrorEnCargarOSacarCreditos = "";
									var lista = OPCreditoLista;
									Console.WriteLine(DateTime.Now.ToString());
									Thread.Sleep(TimeSpan.FromMilliseconds(100));
									Console.WriteLine(DateTime.Now.ToString());
									lista.Add(item);
									OPCreditoLista = lista;
								}
								else
									model.MsgErrorEnCargarOSacarCreditos = "Se ha producido un error al intentar restaurar la lista de Créditos.";
							}
							else
								model.MsgErrorEnCargarOSacarCreditos = respuesta.Entidad.resultado_msj;
						}
						else
							model.MsgErrorEnCargarOSacarCreditos = "Se ha producido un error al intentar generar la transacción de Créditos.";
					}
					else
						model.MsgErrorEnCargarOSacarCreditos = "Se ha producido un error al intentar generar la transacción de Créditos.";

					model.GrillaCreditosNueva = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoNuevaLista);
					return PartialView("_grillaNuevosCreditos", model);
				}
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

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);
		}
		#endregion
	}
}
