using DocumentFormat.OpenXml.Spreadsheet;
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

		private const string accion_agregar = "agregar";
		private const string accion_quitar = "quitar";

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
					Console.WriteLine("Entro 1");
					OPDebitoOriginalLista = datos;
					OPDebitoLista = datos;
					return PartialView("_grillaObligaciones", model);
				}
				else
				{
					Console.WriteLine("Entro 2");
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
					{
						OPDebitoOriginalLista = ObtenerData('D');
						OPDebitoLista = ObtenerData('D');
					}

					var item = OPDebitoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						r.cv_importe = item.cv_imputado;
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de obligaciones
								var listaTemp = OPDebitoLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								DormirMetodo(100);
								OPDebitoLista = listaTemp;

								//Lo agrego a la lista que uso para cargar la grilla de obligaciones nuevas
								var listaAux = new List<OPDebitoYCreditoDelProveedorDto> { item };
								OPDebitoNuevaLista = listaAux;
								model.MsgErrorEnCargarOSacarObligaciones = "";
								if (string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, OPCreditoNuevaLista, accion_agregar);
								}
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
					if (OPCreditoOriginalLista == null || OPCreditoOriginalLista.Count <= 0)
					{
						OPCreditoOriginalLista = ObtenerData('C');
						OPCreditoLista = ObtenerData('C');
					}

					//Busco el elemento en la lista original de creditos
					var item = OPCreditoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
					if (item != null)
					{
						r.cv_importe = item.cv_imputado;
						var respuesta = _ordenDePagoServicio.CargarSacarOPDebitoCreditoDelProveedor(r, TokenCookie);
						Console.WriteLine("Request enviado a [SPGECO_OP_cargar_sacar]:");
						Console.WriteLine(JsonConvert.SerializeObject(r, new JsonSerializerSettings()));

						if (respuesta != null && respuesta.Entidad != null)
						{
							if (respuesta.Entidad.resultado == 0)
							{
								//Lo quito de la lista que uso para cargar la grilla de creditos
								var listaTemp = OPCreditoLista.Where(x => !x.cm_compte_cuota.Equals(r.cuota) && !x.dia_movi.Equals(r.dia_movi) && !x.cm_compte.Equals(r.cm_compte) && !x.tco_id.Equals(r.tco_id) && !x.cta_id.Equals(r.cta_id)).ToList();
								DormirMetodo(100);
								//TODO MARCE: ver si conviene meter un delay para evitar el problema de la actualizacion de las listas en sesion.
								OPCreditoLista = listaTemp;
								//Lo agrego a la lista que uso para cargar la grilla de creditos nuevas
								var listaAux = new List<OPDebitoYCreditoDelProveedorDto> { item };
								OPCreditoNuevaLista = listaAux;
								model.MsgErrorEnCargarOSacarCreditos = "";
								if (string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, OPDebitoNuevaLista, accion_agregar);
								}
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
					{
						OPDebitoOriginalLista = ObtenerData('D');
						OPDebitoLista = ObtenerData('D');
					}

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
								DormirMetodo(100);
								OPDebitoNuevaLista = listaTemp;

								//Si la respuesta contiene valor en la propiedad "rela", debo quitar los items de la lista de creditos nuevos.
								if (string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, OPCreditoNuevaLista, accion_quitar);
								}

								//Lo agrego a la lista de olbigaciones inferior, para eso lo busco en la lista original (Backup)
								item = null;
								item = OPDebitoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
								if (item != null)
								{
									model.MsgErrorEnCargarOSacarObligaciones = "";
									var lista = OPDebitoLista;
									DormirMetodo(100);
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
					if (OPCreditoOriginalLista == null || OPCreditoOriginalLista.Count <= 0)
					{
						OPCreditoOriginalLista = ObtenerData('C');
						OPCreditoLista = ObtenerData('C');
					}

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
								DormirMetodo(100);
								OPCreditoNuevaLista = listaTemp;

								//Si la respuesta contiene valor en la propiedad "rela", debo quitar los items de la lista de creditos nuevos.
								if (string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, OPDebitoNuevaLista, accion_quitar);
								}

								//Lo agrego a la lista de creditos inferior, para eso lo busco en la lista original (Backup)
								item = null;
								item = OPCreditoOriginalLista.FirstOrDefault(x => x.cm_compte_cuota.Equals(r.cuota) && x.dia_movi.Equals(r.dia_movi) && x.cm_compte.Equals(r.cm_compte) && x.tco_id.Equals(r.tco_id) && x.cta_id.Equals(r.cta_id));
								if (item != null)
								{
									model.MsgErrorEnCargarOSacarCreditos = "";
									var lista = OPCreditoLista;
									DormirMetodo(100);
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

		public IActionResult ActualizarGrillaObligacionesInferior()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_grillaObligaciones", ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoLista));
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

		public IActionResult ActualizarGrillaCreditosInferior()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_grillaCreditos", ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoLista));
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

		public JsonResult ActualizarTotalesSuperiores()
		{
			try
			{
				//OPDebitoNuevaLista
				var tot_ObligacionesCancelar = (decimal)0.00;
				if (OPDebitoNuevaLista != null && OPDebitoNuevaLista.Count > 0)
					tot_ObligacionesCancelar = OPDebitoNuevaLista.Sum(x => x.cv_importe);

				//OPCreditoNuevaLista
				var tot_CredYValImputados = (decimal)0.00;
				if (OPCreditoNuevaLista != null && OPCreditoNuevaLista.Count > 0)
					tot_CredYValImputados = OPCreditoNuevaLista.Sum(x => x.cv_importe);
				var tot_Diferencia = tot_ObligacionesCancelar - tot_CredYValImputados;
				return Json(new { error = false, warn = false, msg = string.Empty, data = new TotalesActualizados() { ObligacionesCancelar = tot_ObligacionesCancelar, CredYValImputados = tot_CredYValImputados, Diferencia = tot_Diferencia } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar calcular los totales. {ex}" });
			}
		}

		#region Clases
		public class TotalesActualizados
		{
			public decimal ObligacionesCancelar { get; set; } = 0.00M;
			public decimal CredYValImputados { get; set; } = 0.00M;
			public decimal Diferencia { get; set; } = 0.00M;
		}
		#endregion

		#region Metodos Privados
		/// <summary>
		/// Pausa la ejecución del método actual durante la cantidad de milisegundos especificada.
		/// Es útil para evitar problemas de concurrencia o actualización de listas en sesión, 
		/// especialmente cuando se realizan operaciones que requieren un pequeño retardo para garantizar la consistencia de los datos.
		/// Además, imprime en consola la fecha y hora antes y después de la pausa.
		/// </summary>
		/// <param name="tiempo">Cantidad de tiempo en milisegundos que se debe pausar la ejecución.</param>
		private void DormirMetodo(int tiempo)
		{
			Console.WriteLine(DateTime.Now.ToString());
			Thread.Sleep(TimeSpan.FromMilliseconds(tiempo));
			Console.WriteLine(DateTime.Now.ToString());
		}
		private List<OPDebitoYCreditoDelProveedorDto> ObtenerData(char tipo)
		{
			return _ordenDePagoServicio.GetOPDebitoYCreditoDelProveedor(CtaIdSelected, tipo, false, AdministracionId, UserName, TokenCookie);
		}

		/// <summary>
		/// Actualiza la lista de créditos u obligaciones para agregar, deserializando un JSON recibido y agregando los elementos
		/// que no existan en la lista actual. Si el elemento ya existe (según cta_id, cm_compte, dia_movi y tco_id), no se agrega.
		/// </summary>
		/// <param name="json">Cadena JSON que representa una lista de OPDebitoYCreditoDelProveedorDto a agregar.</param>
		/// <param name="lista">Lista actual de OPDebitoYCreditoDelProveedorDto donde se agregarán/quitaran los nuevos elementos si no existen.</param>
		/// <param name="accion">Lista actual de OPDebitoYCreditoDelProveedorDto donde se agregarán/quitaran los nuevos elementos si no existen.</param>
		private void ActualizarListasDeCreditosObligacionesParaAgregar(string json, List<OPDebitoYCreditoDelProveedorDto> lista, string accion)
		{
			//Deserializo la respuesta
			var listaRela = JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json, new JsonSerializerSettings());
			//Obtengo la lista de creditos
			var listaCreditoObligacionNuevaTemporal = lista;

			//Si hay algo en la lista deserializada
			if (listaRela != null && listaRela.Count > 0)
			{
				if (accion.Equals(accion_agregar))
				{
					foreach (var itemRela in listaRela)
					{
						//Si no existe en la lista que carga la lista de créditos para agregar, lo agrego
						if (!listaCreditoObligacionNuevaTemporal.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id)))
						{
							listaCreditoObligacionNuevaTemporal.Add(itemRela);
						}
					}
				}
				else
				{
					var aux = new List<OPDebitoYCreditoDelProveedorDto>();
					//Si existe en la lista que carga la lista de créditos u obligaciones, lo quito
					foreach (var itemRela in listaRela)
					{
						aux = [.. listaCreditoObligacionNuevaTemporal.Where(x => !x.cta_id.Equals(itemRela.cta_id) && !x.cm_compte.Equals(itemRela.cm_compte) && !x.dia_movi.Equals(itemRela.dia_movi) && !x.tco_id.Equals(itemRela.tco_id))];
						listaCreditoObligacionNuevaTemporal = aux;
					}
				}
			}
			//Actualizo lista
			lista = listaCreditoObligacionNuevaTemporal;
		}
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
