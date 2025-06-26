using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDePagoAProveedorController : OrdenDePagoAProveedorControladorBase
	{
        //PARA MODULO DE IMPRESION
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.OPP.ToString();
        private readonly IDocManagerServicio _docMSv;

        //************************

        private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		private readonly IProveedorServicio _proveedorServicio;
		private readonly IFormaDePagoServicio _formaDePagoServicio;

		private const string tabla_obligaciones = "tbListaObligaciones";
		private const string tabla_creditos = "tbListaCreditos";
		private const string tabla_obligaciones_nuevos = "tbListaObligacionesParaAgregar";
		private const string tabla_creditos_nuevos = "tbListaCreditosParaAgregar";

		private const string accion_agregar = "agregar";
		private const string accion_quitar = "quitar";

		public OrdenDePagoAProveedorController(IOrdenDePagoServicio ordenDePagoServicio, ICuentaServicio cuentaServicio, ITipoGastoServicio tipoGastoServicio, IProveedorServicio proveedorServicio,
											   IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDePagoAProveedorController> logger, 
											   IFormaDePagoServicio formaDePagoServicio, 
											   IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
			_tipoGastoServicio = tipoGastoServicio;
			_proveedorServicio = proveedorServicio;
			_formaDePagoServicio = formaDePagoServicio;

            //PARA MODULO DE IMPRESION
            _docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: OPP
            _docMSv = docManager; //instancio el servicio de impresión

        }

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

                string titulo = "ORDEN DE PAGO A PROVEEDORES"; ;
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables
                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                // en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion


                var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
			
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

				var aux = this.InicializarDatosEnSesion();
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

		/// <summary>
		/// Carga la vista parcial que permite seleccionar obligaciones y créditos para un proveedor específico.
		/// Inicializa los modelos y listas necesarias para la selección, y prepara la interfaz para el usuario.
		/// </summary>
		/// <param name="cta_id">Identificador de la cuenta del proveedor.</param>
		/// <returns>
		/// Una vista parcial con el modelo de selección de obligaciones y créditos, o un mensaje de error si ocurre una excepción.
		/// </returns>
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
				var auxANombreDe = _cuentaServicio.GetFormaDePagoPorCuentaYFP(CtaIdSelected, "H", TokenCookie);
				if (auxANombreDe != null && auxANombreDe.Count > 0 && !string.IsNullOrEmpty(auxANombreDe.First().cta_valores_a_nombre))
					model.valoresANombreDe = auxANombreDe.First().cta_valores_a_nombre;
				else
				{
					var nombreAux = _proveedorServicio.GetProveedorParaABM(cta_id, TokenCookie);
					if (nombreAux != null && nombreAux.Count > 0 && !string.IsNullOrEmpty(nombreAux.First().Cta_Denominacion))
						model.valoresANombreDe = nombreAux.First().Cta_Denominacion;
				}
				return PartialView("_vistaObligYCred_paso1", model);
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
		public IActionResult CargarObligacionesOCreditos(string tipo_carga)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				char tipo;
				if (tipo_carga.Equals("Obligaciones"))
					tipo = 'D'; // Obligaciones
				else
					tipo = 'H'; // Créditos

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
								var listaTemp = new List<OPDebitoYCreditoDelProveedorDto>();
								foreach (var itemDebito in OPDebitoLista)
								{
									if (itemDebito.dia_movi.Equals(r.dia_movi) && itemDebito.cm_compte.Equals(r.cm_compte) && itemDebito.cm_compte_cuota.Equals(r.cuota) && itemDebito.tco_id.Equals(r.tco_id))
										continue;
									listaTemp.Add(itemDebito);
								}
								DormirMetodo(100);
								OPDebitoLista = listaTemp;

								//Lo agrego a la lista que uso para cargar la grilla de obligaciones nuevas
								var listaAux = new List<OPDebitoYCreditoDelProveedorDto>();
								if (OPDebitoNuevaLista != null && OPDebitoNuevaLista.Count >= 0)
								{
									listaAux = OPDebitoNuevaLista;
									listaAux.Add(item);
									OPDebitoNuevaLista = listaAux;
								}
								else
								{
									listaAux = [item];
									OPDebitoNuevaLista = listaAux;
								}

								model.MsgErrorEnCargarOSacarObligaciones = "";
								if (!string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									var listaAux2 = OPCreditoNuevaLista;
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, listaAux2, accion_agregar, true);
									OPCreditoNuevaLista = listaAux2;
								}
							}
							else
							{
								model.MsgErrorEnCargarOSacarObligaciones = string.IsNullOrEmpty(respuesta.Entidad.resultado_msj) ? $"No se puede agregar el item, error desconocido. ({respuesta.Entidad.resultado})" : respuesta.Entidad.resultado_msj;
							}
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
						OPCreditoOriginalLista = ObtenerData('H');
						OPCreditoLista = ObtenerData('H');
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
								//Lo quito de la lista que uso para cargar la grilla de obligaciones
								var listaTemp = new List<OPDebitoYCreditoDelProveedorDto>();
								foreach (var itemCredito in OPCreditoLista)
								{
									if (itemCredito.dia_movi.Equals(r.dia_movi) && itemCredito.cm_compte.Equals(r.cm_compte) && itemCredito.cm_compte_cuota.Equals(r.cuota) && itemCredito.tco_id.Equals(r.tco_id))
										continue;
									listaTemp.Add(itemCredito);
								}
								DormirMetodo(100);
								OPCreditoLista = listaTemp;

								//Lo agrego a la lista que uso para cargar la grilla de creditos nuevas
								var listaAux = new List<OPDebitoYCreditoDelProveedorDto>();
								if (OPCreditoNuevaLista != null && OPCreditoNuevaLista.Count >= 0)
								{
									listaAux = OPCreditoNuevaLista;
									listaAux.Add(item);
									OPCreditoNuevaLista = listaAux;
								}
								else
								{
									listaAux = [item];
									OPCreditoNuevaLista = listaAux;
								}

								model.MsgErrorEnCargarOSacarCreditos = "";
								if (!string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									var listaAux2 = OPDebitoNuevaLista;
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, listaAux2, accion_agregar, false);
									OPDebitoNuevaLista = listaAux2;
								}
							}
							else
							{
								model.MsgErrorEnCargarOSacarCreditos = string.IsNullOrEmpty(respuesta.Entidad.resultado_msj) ? $"No se puede agregar el item, error desconocido. ({respuesta.Entidad.resultado})" : respuesta.Entidad.resultado_msj;
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
								var listaTemp = new List<OPDebitoYCreditoDelProveedorDto>();
								foreach (var itemDebito in OPDebitoNuevaLista)
								{
									if (itemDebito.dia_movi.Equals(r.dia_movi) && itemDebito.cm_compte.Equals(r.cm_compte) && itemDebito.cm_compte_cuota.Equals(r.cuota) && itemDebito.tco_id.Equals(r.tco_id))
										continue;
									listaTemp.Add(itemDebito);
								}
								DormirMetodo(100);
								OPDebitoNuevaLista = listaTemp;

								//Si la respuesta contiene valor en la propiedad "rela", debo quitar los items de la lista de creditos nuevos.
								if (!string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									var listaAux2 = OPCreditoNuevaLista;
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, listaAux2, accion_quitar, true);
									//OPCreditoNuevaLista = listaAux2;

									//Si estos items de la propiedad rela ya existian en la lista original de Créditos, debo agregarlos como forma de restauración.
									if (OPCreditoOriginalLista == null || OPCreditoOriginalLista.Count <= 0)
									{
										OPCreditoOriginalLista = ObtenerData('H');
									}
									OPCreditoLista = ActualizarListasDeCreditosObligaciones(respuesta.Entidad.rela, OPCreditoOriginalLista, OPCreditoLista);
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
								model.MsgErrorEnCargarOSacarObligaciones = string.IsNullOrEmpty(respuesta.Entidad.resultado_msj) ? $"No se puede agregar el item, error desconocido. ({respuesta.Entidad.resultado})" : respuesta.Entidad.resultado_msj;
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
						OPCreditoOriginalLista = ObtenerData('H');
						OPCreditoLista = ObtenerData('H');
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
								var listaTemp = new List<OPDebitoYCreditoDelProveedorDto>();
								foreach (var itemCredito in OPCreditoNuevaLista)
								{
									if (itemCredito.dia_movi.Equals(r.dia_movi) && itemCredito.cm_compte.Equals(r.cm_compte) && itemCredito.cm_compte_cuota.Equals(r.cuota) && itemCredito.tco_id.Equals(r.tco_id))
										continue;
									listaTemp.Add(itemCredito);
								}
								DormirMetodo(100);
								OPCreditoNuevaLista = listaTemp;

								//Si la respuesta contiene valor en la propiedad "rela", debo quitar los items de la lista de creditos nuevos.
								if (!string.IsNullOrEmpty(respuesta.Entidad.rela.Trim()))
								{
									var listaAux2 = OPDebitoNuevaLista;
									ActualizarListasDeCreditosObligacionesParaAgregar(respuesta.Entidad.rela, listaAux2, accion_quitar, false);
									//OPDebitoNuevaLista = listaAux2;

									//Si estos items de la propiedad rela ya existian en la lista original de Débitos, debo agregarlos como forma de restauración.
									if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
									{
										OPDebitoOriginalLista = ObtenerData('D');
									}
									OPDebitoLista = ActualizarListasDeCreditosObligaciones(respuesta.Entidad.rela, OPDebitoOriginalLista, OPDebitoLista);
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
								model.MsgErrorEnCargarOSacarCreditos = string.IsNullOrEmpty(respuesta.Entidad.resultado_msj) ? $"No se puede agregar el item, error desconocido. ({respuesta.Entidad.resultado})" : respuesta.Entidad.resultado_msj;
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

		public IActionResult ActualizarGrillaCreditosSuperior()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var model = new CargarNuevosCreditosModel
				{
					GrillaCreditosNueva = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoNuevaLista)
				};
				return PartialView("_grillaNuevosCreditos", model);
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

		public IActionResult ActualizarGrillaObligacionesSuperior()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var model = new CargarNuevasObligacionesModel
				{
					GrillaObligacionesNuevas = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoNuevaLista)
				};
				return PartialView("_grillaNuevosObligaciones", model);
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
					tot_CredYValImputados += OPCreditoNuevaLista.Sum(x => x.cv_importe);
				if (OPRetencionesDesdeObligYCredLista != null && OPRetencionesDesdeObligYCredLista.Count > 0)
					tot_CredYValImputados += OPRetencionesDesdeObligYCredLista.Sum(x => x.retencion);
				if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0)
					tot_CredYValImputados += OPValoresDesdeObligYCredLista.Sum(x => x.op_importe);
				var tot_Diferencia = tot_ObligacionesCancelar - tot_CredYValImputados;
				return Json(new { error = false, warn = false, msg = string.Empty, data = new TotalesActualizados() { ObligacionesCancelar = tot_ObligacionesCancelar, CredYValImputados = tot_CredYValImputados, Diferencia = tot_Diferencia } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar calcular los totales. {ex}" });
			}
		}

		[HttpPost]
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				CtaIdSelected = "";
				OPValidacionPrevLista = [];
				OPDebitoLista = [];
				OPDebitoOriginalLista = [];
				OPDebitoNuevaLista = [];
				OPCreditoLista = [];
				OPCreditoOriginalLista = [];
				OPCreditoNuevaLista = [];
				OPRetencionesDesdeObligYCredLista = [];
				OPValoresDesdeObligYCredLista = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - ORDENDECOMPRA" });
			}
		}

		[HttpPost]
		public IActionResult CargarVistaObligacionesYCreditosPaso1()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var model = new CargarObligacionesOCreditosModel
				{
					ctaDir = "",
					listaCtaDir = ComboTipoGasto(),
					GrillaCreditosNueva = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoNuevaLista),
					GrillaObligacionesNuevas = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoNuevaLista)
				};
				return PartialView("_vistaObligYCred_paso1", model);
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
		public IActionResult CargarVistaObligacionesYCreditosPaso2()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var cuentaObs = string.Empty;
				var observacionesList = _cuentaServicio.ObtenerCuentaObs(CtaIdSelected, 'P', TokenCookie);
				if (observacionesList != null && observacionesList.Count > 0)
				{
					cuentaObs = observacionesList.FirstOrDefault()?.cta_obs ?? string.Empty;
				}
				var cfp = _cuentaServicio.GetCuentaFormaDePago(CtaIdSelected, TokenCookie);
				//var cfp2 = new List<CuentaFPDto>();
				var model = new CargarObligacionesOCreditosPaso2Model
				{
					GrillaCreditosNueva = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPCreditoNuevaLista),
					GrillaObligacionesNuevas = ObtenerGridCoreSmart<OPDebitoYCreditoDelProveedorDto>(OPDebitoNuevaLista),
					GrillaRetenciones = new GridCoreSmart<RetencionesDesdeObligYCredDto>(),
					GrillaValores = new GridCoreSmart<ValoresDesdeObligYCredDto>(),
					GrillaMedioDePago = ObtenerGridCoreSmart<CuentaFPDto>(cfp),
					EsPagoAnticipado = OPDebitoNuevaLista.Count == 0,
					TieneMediosDePagos = cfp.Count > 0,
					CuentaObs = cuentaObs,
				};
				if (model.EsPagoAnticipado)
				{
					if (OPDebitoOriginalLista == null || OPDebitoOriginalLista.Count <= 0)
					{
						OPDebitoOriginalLista = ObtenerData('D');
						OPDebitoLista = ObtenerData('D');
					}
					if (OPCreditoOriginalLista == null || OPCreditoOriginalLista.Count <= 0)
					{
						OPCreditoOriginalLista = ObtenerData('H');
						OPCreditoLista = ObtenerData('H');
					}
				}
				return PartialView("_vistaObligYCred_paso2", model);
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
		public IActionResult CargarRetencionesDesdeObligYCredSeleccionados()
		{
			var model = new GridCoreSmart<RetencionesDesdeObligYCredDto>();
			try
			{
				var cta_id = CtaIdSelected;
				var json_debitos = JsonConvert.SerializeObject(OPDebitoNuevaLista, new JsonSerializerSettings());
				var json_creditos = JsonConvert.SerializeObject(OPCreditoNuevaLista, new JsonSerializerSettings());
				var response = _ordenDePagoServicio.CargarRetencionesDesdeObligYCredSeleccionados(new CargarRetencionesDesdeObligYCredSeleccionadosRequest
				{
					cta_id = cta_id,
					json_d = json_debitos,
					json_h = json_creditos
				}, TokenCookie);
				model = ObtenerGridCoreSmart<RetencionesDesdeObligYCredDto>(response);
				OPRetencionesDesdeObligYCredLista = response;
				return PartialView("_grillaRetenciones", model);
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
		public IActionResult CargarValoresDesdeObligYCredSeleccionados()
		{
			var model = new GridCoreSmart<ValoresDesdeObligYCredDto>();
			try
			{
				var cta_id = CtaIdSelected;
				var json_debitos = JsonConvert.SerializeObject(OPDebitoNuevaLista, new JsonSerializerSettings());
				var json_creditos = JsonConvert.SerializeObject(OPCreditoNuevaLista, new JsonSerializerSettings());
				var response = _ordenDePagoServicio.CargarValoresDesdeObligYCredSeleccionados(new CargarValoresDesdeObligYCredSeleccionadosRequest
				{
					cta_id = cta_id,
					json_d = json_debitos,
					json_h = json_creditos
				}, TokenCookie);
				var listaAux = OPValoresDesdeObligYCredLista;
				listaAux.AddRange(response);
				RecalcularOrden(listaAux);
				OPValoresDesdeObligYCredLista = listaAux;
				model = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDesdeObligYCredLista);
				return PartialView("_grillaValores", model);
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
		public IActionResult ActualizarGrillaValores(int orden)
		{
			var model = new GridCoreSmart<ValoresDesdeObligYCredDto>();
			try
			{
				if (orden > 0)
				{
					var listaAux = OPValoresDesdeObligYCredLista;
					listaAux = [.. listaAux.Where(x => x.orden != orden)];
					OPValoresDesdeObligYCredLista = listaAux;
				}
				model = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDesdeObligYCredLista);
				return PartialView("_grillaValores", model);
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
		public IActionResult ValidarAntesDeConfirmar()
		{
			try
			{
				//Validar que las obligaciones o débitos sean mayores a cero y su total de imputación sea igual al total de créditos + retenciones + valores. 
				//Se comenta este codigo por orden de CR en task 20250607 - Revisión Orden de Pago Proveedores
				//if (OPDebitoNuevaLista != null && OPDebitoNuevaLista.Count > 0 && OPDebitoNuevaLista.Sum(x => x.cv_importe) > 0)
				//{
				//	var total_h = ObtenerTotalImputacionCreditos();
				//	if (total_h != OPDebitoNuevaLista.Sum(x => x.cv_importe))
				//	{
				//		return Json(new { error = true, warn = false, msg = "Las Obligaciones (Débitos) deben ser igual a la suma de Créditos + Retenciones + Valores." });
				//	}
				//}
				//Salvo, que sea un pago anticipado, en este caso las obligaciones, créditos y retenciones deben ser iguales a cero y los valores emitidos mayores a cero.
				if (OPDebitoNuevaLista != null && OPDebitoNuevaLista.Count == 0)
				{
					if (OPCreditoNuevaLista != null && OPCreditoNuevaLista.Count > 0)
					{
						return Json(new { error = true, warn = false, msg = "Para generar un pago anticipado, los Créditos deben ser 0." });
					}
					if (OPRetencionesDesdeObligYCredLista != null && OPRetencionesDesdeObligYCredLista.Count > 0)
					{
						return Json(new { error = true, warn = false, msg = "Para generar un pago anticipado, las retenciones deben ser 0." });
					}
					if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count <= 0)
					{
						return Json(new { error = true, warn = false, msg = "Para generar un pago anticipado, debe especificar un Valor." });
					}
				}
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar hacer las validaciones previas a la cofirmación. {ex}" });
			}
		}

		[HttpPost]
		public JsonResult ConfirmarOPaProveedor(ConfirmarOPaProveedorRequest req)
		{
			try
			{
				if (req == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío, por favor revise." });

				req.cta_id = CtaIdSelected;
				req.adm_id = AdministracionId;
				req.usu_id = UserName;
				req.json_d = JsonConvert.SerializeObject(OPDebitoNuevaLista, new JsonSerializerSettings());
				req.json_h = JsonConvert.SerializeObject(OPCreditoNuevaLista, new JsonSerializerSettings());
				req.json_r = JsonConvert.SerializeObject(OPRetencionesDesdeObligYCredLista, new JsonSerializerSettings());
				req.json_v = JsonConvert.SerializeObject(OPValoresDesdeObligYCredLista, new JsonSerializerSettings());

				Console.WriteLine("Request enviado a [SPGECO_OP_Confirmar]:");
				Console.WriteLine("op_desc:");
				Console.WriteLine(req.op_desc);
				Console.WriteLine("opt_id:");
				Console.WriteLine(req.opt_id);
				Console.WriteLine("cta_id:");
				Console.WriteLine(CtaIdSelected);
				Console.WriteLine("adm_id:");
				Console.WriteLine(AdministracionId);
				Console.WriteLine("usu_id:");
				Console.WriteLine(UserName);
				Console.WriteLine("json_d:");
				Console.WriteLine(JsonConvert.SerializeObject(OPDebitoNuevaLista, new JsonSerializerSettings()));
				Console.WriteLine("json_h:");
				Console.WriteLine(JsonConvert.SerializeObject(OPCreditoNuevaLista, new JsonSerializerSettings()));
				Console.WriteLine("json_r:");
				Console.WriteLine(JsonConvert.SerializeObject(OPRetencionesDesdeObligYCredLista, new JsonSerializerSettings()));
				Console.WriteLine("json_v:");
				Console.WriteLine(JsonConvert.SerializeObject(OPValoresDesdeObligYCredLista, new JsonSerializerSettings()));
				var respuesta = _ordenDePagoServicio.ConfirmarOrdenDePagoAProveedor(req, TokenCookie);
				return AnalizarRespuesta(respuesta, "La Orden de Compra se Confirmo con Éxito");
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al confirmar la Orden de Pago a Proveedor. {ex}" });
			}
		}

		#region Clases
		private void RecalcularOrden(List<ValoresDesdeObligYCredDto> lista)
		{
			if (lista != null && lista.Count > 0)
			{
				var index = 0;
				foreach (var item in lista)
				{
					index++;
					item.orden = index;
				}
			}
		}

		public class TotalesActualizados
		{
			public decimal ObligacionesCancelar { get; set; } = 0.00M;
			public decimal CredYValImputados { get; set; } = 0.00M;
			public decimal Diferencia { get; set; } = 0.00M;
		}
		#endregion

		#region Metodos Privados
		private decimal ObtenerTotalImputacionCreditos()
		{
			try
			{
				var tot_CredYValImputados = (decimal)0.00;
				if (OPCreditoNuevaLista != null && OPCreditoNuevaLista.Count > 0)
					tot_CredYValImputados += OPCreditoNuevaLista.Sum(x => x.cv_importe);
				if (OPRetencionesDesdeObligYCredLista != null && OPRetencionesDesdeObligYCredLista.Count > 0)
					tot_CredYValImputados += OPRetencionesDesdeObligYCredLista.Sum(x => x.retencion);
				if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0)
					tot_CredYValImputados += OPValoresDesdeObligYCredLista.Sum(x => x.op_importe);
				return tot_CredYValImputados;
			}
			catch (Exception)
			{
				return 0.00M;
			}

		}


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

		private List<OPDebitoYCreditoDelProveedorDto> ActualizarListasDeCreditosObligaciones(string json, List<OPDebitoYCreditoDelProveedorDto> listaOriginal, List<OPDebitoYCreditoDelProveedorDto> lista)
		{
			//Deserializo la respuesta
			var listaRela = JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json, new JsonSerializerSettings());
			var listaOriginalAux = listaOriginal;
			var listaAux = lista;

			if (listaRela != null && listaRela.Count > 0)
			{
				foreach (var itemRela in listaRela)
				{
					var existeEnListaOriginal = listaOriginalAux.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id));
					var noExisteEnLista = !listaAux.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id));
					if (existeEnListaOriginal && noExisteEnLista)
					{
						listaAux.Add(itemRela);
					}
				}
				lista = listaAux;
			}
			return listaAux;
		}

		/// <summary>
		/// Actualiza la lista de créditos u obligaciones para agregar, deserializando un JSON recibido y agregando los elementos
		/// que no existan en la lista actual. Si el elemento ya existe (según cta_id, cm_compte, dia_movi y tco_id), no se agrega.
		/// </summary>
		/// <param name="json">Cadena JSON que representa una lista de OPDebitoYCreditoDelProveedorDto a agregar.</param>
		/// <param name="lista">Lista actual de OPDebitoYCreditoDelProveedorDto donde se agregarán/quitaran los nuevos elementos si no existen.</param>
		/// <param name="accion">Lista actual de OPDebitoYCreditoDelProveedorDto donde se agregarán/quitaran los nuevos elementos si no existen.</param>
		private void ActualizarListasDeCreditosObligacionesParaAgregar(string json, List<OPDebitoYCreditoDelProveedorDto> lista, string accion, bool esCredito = false)
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
						//Si no existe en la lista que carga la lista de créditos/débitos para agregar, y existen en la grilla inferior, lo agrego
						var noExisteEnListaNueva = !listaCreditoObligacionNuevaTemporal.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id));
						var existe = false;
						if (esCredito)
						{
							if (OPCreditoLista == null || OPCreditoLista.Count <= 0)
							{
								OPCreditoNuevaLista = ObtenerData('H');
								OPCreditoLista = ObtenerData('H');
							}
							existe = OPCreditoLista.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id));
						}
						else
						{
							if (OPDebitoLista == null || OPDebitoLista.Count <= 0)
							{
								OPDebitoOriginalLista = ObtenerData('D');
								OPDebitoLista = ObtenerData('D');
							}
							existe = OPDebitoLista.Exists(x => x.cta_id.Equals(itemRela.cta_id) && x.cm_compte.Equals(itemRela.cm_compte) && x.dia_movi.Equals(itemRela.dia_movi) && x.tco_id.Equals(itemRela.tco_id));
						}
						if (noExisteEnListaNueva && existe)
						{
							listaCreditoObligacionNuevaTemporal.Add(itemRela);
							var listaAux = new List<OPDebitoYCreditoDelProveedorDto>();
							//Si existe, lo quito de la lista de Creditos existentes
							if (esCredito)
							{
								foreach (var item in OPCreditoLista)
								{
									if (item.cm_compte == itemRela.cm_compte && item.dia_movi == itemRela.dia_movi && item.tco_id == itemRela.tco_id)
										continue;
									listaAux.Add(item);
								}
								OPCreditoLista = listaAux;
							}
							else//Si existe, lo quito de la lista de Obligaciones existentes
							{
								foreach (var item in OPDebitoLista)
								{
									if (item.cm_compte == itemRela.cm_compte && item.dia_movi == itemRela.dia_movi && item.tco_id == itemRela.tco_id)
										continue;
									listaAux.Add(item);
								}
								OPDebitoLista = listaAux;
							}
						}
					}
				}
				else
				{
					var aux = new List<OPDebitoYCreditoDelProveedorDto>();
					//Si existe en la lista que carga la lista de créditos u obligaciones, lo quito
					foreach (var item in listaCreditoObligacionNuevaTemporal)
					{
						if (listaRela.Exists(x => x.cta_id.Equals(item.cta_id) && x.cm_compte.Equals(item.cm_compte) && x.dia_movi.Equals(item.dia_movi) && x.tco_id.Equals(item.tco_id)))
							continue;
						aux.Add(item);
					}
					if (esCredito)
						OPCreditoNuevaLista = aux;
					else
						OPDebitoNuevaLista = aux;
					listaCreditoObligacionNuevaTemporal = aux;
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
