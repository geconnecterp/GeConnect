using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.InventarioCarga;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Tipos;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class InventarioCargaController : InventarioCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IInventarioServicio _inventarioServicio;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IInventarioEstadoServicio _inventarioEstadoServicio;
		private readonly ISectorServicio _sectorServicio;
		private readonly IUserServicio _userServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly ITipoInventarioServicio _tipoInventarioServicio;
		public InventarioCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioCargaController> logger,
										 IInventarioServicio inventarioServicio, IDepositoServicio depositoServicio, IInventarioEstadoServicio inventarioEstadoServicio,
										 ISectorServicio sectorServicio, IUserServicio userServicio, IRubroServicio rubroServicio, ITipoInventarioServicio tipoInventarioServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
			_depositoServicio = depositoServicio;
			_inventarioEstadoServicio = inventarioEstadoServicio;
			_sectorServicio = sectorServicio;
			_userServicio = userServicio;
			_rubroServicio = rubroServicio;
			_tipoInventarioServicio = tipoInventarioServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "INVENTARIOS";
				ViewData["Titulo"] = titulo;

				InicializarDatosDeSession();

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

		public IActionResult InicializarPantallPrincipal()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_inventarioCargaPantallaPrincipal");
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult BuscarInventarioLista(GetInventarioListaRequest request)
		{
			var model = new InventarioCargaGrillaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				request.adm_id = AdministracionId;
				request.usu_id = "%";
				request.inve_id = "%";
				var lista = _inventarioServicio.GetInventarioLista(request, TokenCookie);
				model.GrillaInventario = ObtenerGridCoreSmart<InventarioListaDto>(lista);
				ListaInventario = lista;
				return PartialView("_gridInventarioCarga", model);
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

		public IActionResult CargarCamposDatosInventario(string inv_nro = "")
		{
			var model = new InventarioCargaDatosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (inv_nro != "")
				{
					//var inventario = ListaInventario.FirstOrDefault(x => x.inv_nro == inv_nro);
					var inventarios = _inventarioServicio.GetInventarioDatos(new GetInventarioDatosRequest() { inv_nro = inv_nro }, TokenCookie);
					if (inventarios != null && inventarios.Count>0)
					{
						var inventario = inventarios.First();
						model.inv_nro = inventario.inv_nro;
						model.Descripcion = inventario.inv_descripcion;
						model.AS_N = inventario.as_nro;
						model.Estado = inventario.inve_desc;
						model.AperturaDesde = inventario.inv_apertura;
						model.AperturaHasta = inventario.inv_cierre;
						model.DepositoSeleccionado = inventario.depo_id;
						model.ConteoSeleccionado = inventario.invt_id?.ToString();
					}
				}
				else {
					model.AperturaDesde = DateTime.Now.AddDays(-1);
					model.AperturaHasta = DateTime.Now;
					model.Descripcion = string.Empty;
					model.Estado = string.Empty;
				}
				//var estados = _inventarioEstadoServicio.GetInventarioEstadoLista(TokenCookie);
				var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
				if (depositos != null && depositos.Count > 0)
					model.ListaDepositos = ObtenerListaDepositos(depositos);
				else
					model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var conteos = ObtenerListaConteos();
				model.ListaConteos = conteos;
				return PartialView("_inventarioDatos", model);
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

		public IActionResult CargarDatosAdicionalesInicial()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_grillasAdicionales");
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult CargarGrillaRubrosEnSeccionDatosAdicionales(string inv_nro)
		{
			var model = new InventarioCargaGrillaRubrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _inventarioServicio.GetRubrosEnInventario(inv_nro, TokenCookie, AdministracionId);
				model.GrillaRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(lista);
				ListaRubroEnInventario = lista;
				return PartialView("_grillasAdicionalesRubros", model);
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

		//public IActionResult CargarDatosDeInvEnSeccionDatosAdicionales(string inv_nro)
		//{ 

		//}

		public IActionResult CargarGrillaUsuariosEnSeccionDatosAdicionales(string inv_nro)
		{
			var model = new InventarioCargaGrillaUsuariosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _inventarioServicio.GetUsuariosEnInventario(inv_nro, TokenCookie);
				ListaUsuarioEnInventario = lista;
				model.GrillaUsuarios = ObtenerGridCoreSmart<UsuarioEnInventarioDto>(lista);
				return PartialView("_grillasAdicionalesUsuarios", model);
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

		public IActionResult CargarListaSectoresEnSeccionDatosAdicionales()
		{
			var model = new ListaSectorModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _sectorServicio.GetSectoresLista(TokenCookie);
				model.ListaSectores = ObtenerListaSectores(lista);
				return PartialView("_listaSectores", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult CargarListaUsuariosEnSeccionDatosAdicionales()
		{
			var model = new ListaUsuarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _userServicio.BuscarUsuarioLista(AdministracionId, TokenCookie).Result;
				ListaUsuarios = lista.ListaEntidad ?? [];
				model.ListaUsuarios = ObtenerListaUsuarios(lista.ListaEntidad ?? []);
				return PartialView("_listaUsuarios", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult CargarListaRubrosEnSeccionDatosAdicionales()
		{
			var model = new ListaRubroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _rubroServicio.ObtenerListaRubros("%", TokenCookie);
				ListaRubros = lista;
				model.ListaRubros = ObtenerListaRubros(lista ?? []);
				return PartialView("_listaRubros", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult AgregarRubrosPorSector(string sec_id)
		{
			var model = new InventarioCargaGrillaRubrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(sec_id))
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						Mensaje = "No se ha especificado un valor para sec_id"
					};
					return PartialView("_gridMensaje", response);
				}
				var rubrosEnSector = _sectorServicio.GetRubroParaABM(sec_id, TokenCookie);
				var listaRubrosMapeada = RubrosMapper(rubrosEnSector);
				AgregarRubrosAListaRubroEnInventario(listaRubrosMapeada);
				model.GrillaRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(ListaRubroEnInventario);
				return PartialView("_grillasAdicionalesRubros", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult AgregarRubroIndividual(string rub_id)
		{
			var model = new InventarioCargaGrillaRubrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(rub_id))
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						Mensaje = "No se ha especificado un valor para rub_id"
					};
					return PartialView("_gridMensaje", response);
				}
				var rubro = ListaRubroEnInventario.FirstOrDefault(x => x.rub_id == rub_id);
				if (rubro != null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						Mensaje = "El rubro que intenta agregar ya existe."
					};
					return PartialView("_gridMensaje", response);
				}
				var rubroMapeado = RubrosMapperIndividual(ListaRubros.Where(x => x.Rub_Id == rub_id).First());
				AgregarRubrosAListaRubroEnInventario(rubroMapeado);
				model.GrillaRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(ListaRubroEnInventario);
				return PartialView("_grillasAdicionalesRubros", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult AgregarUsuarioIndividual(string usu_id, string grupo)
		{
			var model = new InventarioCargaGrillaUsuariosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (ListaUsuarios == null || ListaUsuarios.Count <= 0)
					RecargarUsuarios();

				//FixListaUsuariosEnInventarioEnControllerBase(request.inv_nro);

				if (ListaUsuarioEnInventario.Where(x => x.usu_id == usu_id).Any())
				{
					model.GrillaUsuarios = ObtenerGridCoreSmart<UsuarioEnInventarioDto>(ListaUsuarioEnInventario);
					return PartialView("_grillasAdicionalesUsuarios", model);
				}

				var listaTemp = ListaUsuarioEnInventario;
				var item = ListaUsuarios.Where(x => x.usu_id == usu_id).FirstOrDefault();
				if (item != null)
				{
					var newItem = new UsuarioEnInventarioDto()
					{
						usu_id = usu_id,
						usu_apellidoynombre = item.usu_apellidoynombre,
						inv_descripcion = string.Empty,
						inv_grupo = grupo,
						inv_nro = string.Empty
					};
					listaTemp.Add(newItem);
				}
				ListaUsuarioEnInventario = listaTemp;
				model.GrillaUsuarios = ObtenerGridCoreSmart<UsuarioEnInventarioDto>(ListaUsuarioEnInventario);
				return PartialView("_grillasAdicionalesUsuarios", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public JsonResult ConfirmarInventario(ConfirmarInventarioRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, ok=false, msg = "No autenticado" });

				FixListaUsuariosEnInventarioEnControllerBase(request.inv_nro);

				if (string.IsNullOrEmpty(request.inv_nro))
					request.inv_nro = "";
				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				request.json_r = ObtenerJsonRubroParaConfirmacionDeInventario();
				request.json_u = ObtenerJsonUsuarioParaConfirmacionDeInventario();
				PrintProperties(request);
				var respuesta = _inventarioServicio.ConfirmarInventario(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La acción se ejecutó correctamente.");
				//return Json(respuesta);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return Json(response);
			}
		}

		public JsonResult RegistrarStockDeControl(RegistrarStockDeControlRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, ok = false, msg = "No autenticado" });

				if (string.IsNullOrEmpty(request.inv_nro))
					return Json(new { error = true, warn = false, ok = false, msg = "Request inválido" });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				request.inv_nro = request.inv_nro;
				PrintProperties(request);
				var respuesta = _inventarioServicio.RegistrarControlDeStock(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La acción se ejecutó correctamente.");
				//return Json(respuesta);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return Json(response);
			}
		}

		public IActionResult QuitarItemEnGrillaRubro(string inv_nro, string rub_id)
		{
			var model = new InventarioCargaGrillaRubrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaTemp = ListaRubroEnInventario;
				listaTemp = [.. listaTemp.Where(x => x.rub_id != rub_id)];
				ListaRubroEnInventario = listaTemp;
				model.GrillaRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(listaTemp);
				return PartialView("_grillasAdicionalesRubros", model);
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

		public IActionResult QuitarItemEnGrillaUsuarios(string inv_nro, string usr_id)
		{
			var model = new InventarioCargaGrillaUsuariosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaTemp = ListaUsuarioEnInventario;
				listaTemp = [.. listaTemp.Where(x => x.usu_id != usr_id)];
				ListaUsuarioEnInventario = listaTemp;
				model.GrillaUsuarios = ObtenerGridCoreSmart<UsuarioEnInventarioDto>(listaTemp);
				return PartialView("_grillasAdicionalesUsuarios", model);
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

		public IActionResult InicializarTabValorizacion(string inv_nro, string invt_id)
		{
			var model = new ValorizacionInventarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var inv = ListaInventario.Where(x => x.inv_nro == inv_nro).ToList();
				if (inv == null || inv.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						Mensaje = "No se han encontrado datos para realizar la valorización del inventario seleccionado."
					};
					return PartialView("_gridMensaje", response);
				}

				if (invt_id == "D" || invt_id == "S")
				{
					var listaRubro = _inventarioServicio.GetRubrosEnInventario(inv_nro, TokenCookie);
					model.GrillaInvRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(listaRubro);
					model.EsTipoBox = false;
				}
				else 
				{
					var listaBox = _inventarioServicio.GetInventarioBox(new InventarioRequestDto() { inv_nro = inv_nro, usu_id = "%" }, TokenCookie).Result;
					model.GrillaInvBoxes = ObtenerGridCoreSmart<InventarioBoxDto>(listaBox.ListaEntidad ?? []);
					model.EsTipoBox = true;
				}
				var invSeleccionado = inv.First();
				model.inv_nro = invSeleccionado.inv_nro;
				model.inv_descripcion = invSeleccionado.inv_descripcion;
				return PartialView("_valorizacionInventario", model);
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

		#region Metodos privados
		public void FixListaUsuariosEnInventarioEnControllerBase(string inv_nro)
		{
			//Fix: por que la bosta de las variables de sesion se tiran pedos por los ojos cuando las cosas se hace rapido:
			if (ListaUsuarioEnInventario == null || ListaUsuarioEnInventario.Count <= 0)
			{
				var lista = _inventarioServicio.GetUsuariosEnInventario(inv_nro, TokenCookie);
				ListaUsuarioEnInventario = lista;
			}
		}
		private string ObtenerJsonUsuarioParaConfirmacionDeInventario()
		{
			var listaParaConfirmacion = new List<UsuarioParaConfirmacion>();
			foreach (var usuario in ListaUsuarioEnInventario)
			{
				var usuarioConf = new UsuarioParaConfirmacion
				{
					usu_id = usuario.usu_id,
					inv_grupo = usuario.inv_grupo
				};
				listaParaConfirmacion.Add(usuarioConf);
			}
			var json = JsonConvert.SerializeObject(listaParaConfirmacion);
			return json;
		}
		private string ObtenerJsonRubroParaConfirmacionDeInventario()
		{
			var listaParaConfirmacion = new List<RubroParaConfirmacion>();
			foreach (var rubro in ListaRubroEnInventario)
			{
				var rubroConf = new RubroParaConfirmacion
				{
					rub_id = rubro.rub_id,
					cta_id = rubro.cta_id
				};
				listaParaConfirmacion.Add(rubroConf);
			}
			var json = JsonConvert.SerializeObject(listaParaConfirmacion);
			return json;
		}
		private void RecargarUsuarios()
		{
			var lista = _userServicio.BuscarUsuarioLista(AdministracionId, TokenCookie).Result;
			ListaUsuarios = lista.ListaEntidad ?? [];
		}
		private void AgregarRubrosAListaRubroEnInventario(List<RubroEnInventarioDto> rubrosAAgregar)
		{
			var listaTemp = ListaRubroEnInventario;
			foreach (var rubro in rubrosAAgregar)
			{
				var existeRubro = listaTemp.Any(x => x.rub_id == rubro.rub_id);
				if (!existeRubro)
				{
					listaTemp.Add(rubro);
				}
			}
			ListaRubroEnInventario = listaTemp;
		}
		private List<RubroEnInventarioDto> RubrosMapper(List<RubroListaABMDto> rubros)
		{
			var lista = new List<RubroEnInventarioDto>();
			foreach (var rubro in rubros)
			{
				var rubroEnInventario = new RubroEnInventarioDto
				{
					inv_nro = string.Empty,
					inv_descripcion = string.Empty,
					rub_id = rubro.Rub_Id,
					rub_desc = rubro.Rub_Desc,
					cta_id = "%",
					cant_prod_stk = 0,
					cant_prod_stk_positivo = 0,
					cant_prod_conteo = 0
				};
				lista.Add(rubroEnInventario);
			}
			return lista;
		}
		private List<RubroEnInventarioDto> RubrosMapperIndividual(RubroListaDto rubro)
		{
			var lista = new List<RubroEnInventarioDto>();
			var rubroEnInventario = new RubroEnInventarioDto
			{
				inv_nro = string.Empty,
				inv_descripcion = string.Empty,
				rub_id = rubro.Rub_Id,
				rub_desc = rubro.Rub_Desc,
				cta_id = "%",
				cant_prod_stk = 0,
				cant_prod_stk_positivo = 0,
				cant_prod_conteo = 0
			};
			lista.Add(rubroEnInventario);
			return lista;
		}
		private SelectList ObtenerListaConteos()
		{
			var tipoInventarioLista = _tipoInventarioServicio.GetTiposEnventario(TokenCookie);
			if (tipoInventarioLista != null && tipoInventarioLista.Count > 0)
			{
				var lista = tipoInventarioLista.Select(x => new ComboGenDto { Id = x.invt_id.ToString(), Descripcion = x.invt_desc });
				return HelperMvc<ComboGenDto>.ListaGenerica(lista);
			}
			else
			{
				return HelperMvc<ComboGenDto>.ListaGenerica([]);
			}	
		}
		private SelectList ObtenerListaRubros(List<RubroListaDto> rub)
		{
			var lista = rub.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaUsuarios(List<UserDto> usr)
		{
			var lista = usr.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = x.usu_apellidoynombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaEstados(List<InventarioEstadoDto> estados)
		{
			var lista = estados.Select(x => new ComboGenDto { Id = x.inve_id, Descripcion = x.inve_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaSectores(List<SectorDto> sectores)
		{
			var lista = sectores.Select(x => new ComboGenDto { Id = x.Sec_Id, Descripcion = x.Sec_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private void InicializarDatosDeSession()
		{
			try
			{
				ListaInventario = [];
				ListaRubroEnInventario = [];
				ListaUsuarioEnInventario = [];
				ListaRubros = [];
				ListaUsuarios = [];
			}
			catch (Exception)
			{
			}
		}
		#endregion

		#region Clases privadas auxiliares
		private class  RubroParaConfirmacion
		{
			public string rub_id { get; set; } = string.Empty;
			public string cta_id { get; set; } = "%";
		}
		private class UsuarioParaConfirmacion
		{
			public string usu_id { get; set; } = string.Empty;
			public string inv_grupo { get; set; } = string.Empty;
		}
		#endregion
	}
}
