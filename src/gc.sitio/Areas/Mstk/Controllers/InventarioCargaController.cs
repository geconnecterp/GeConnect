using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.InventarioCarga;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Tipos;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

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
		public InventarioCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioCargaController> logger,
										 IInventarioServicio inventarioServicio, IDepositoServicio depositoServicio, IInventarioEstadoServicio inventarioEstadoServicio,
										 ISectorServicio sectorServicio, IUserServicio userServicio, IRubroServicio rubroServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
			_depositoServicio = depositoServicio;
			_inventarioEstadoServicio = inventarioEstadoServicio;
			_sectorServicio = sectorServicio;
			_userServicio = userServicio;
			_rubroServicio = rubroServicio;
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
				request.usu_id = UserName;
				request.inve_id = "%";
				var lista = _inventarioServicio.GetInventarioLista(request, Token);
				model.GrillaInventario = ObtenerGridCoreSmart<InventarioListaDto>(lista);
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

		public IActionResult CargarCamposDatosInventario()
		{
			var model = new InventarioCargaDatosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var estados = _inventarioEstadoServicio.GetInventarioEstadoLista(TokenCookie);
				if (estados != null && estados.Count > 0)
					model.ListaEstado = ObtenerListaEstados(estados);
				else
					model.ListaEstado = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
				if (depositos != null && depositos.Count > 0)
					model.ListaDepositos = ObtenerListaDepositos(depositos);
				else
					model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var conteos = ObtenerListaConteos();
				model.ListaConteos = conteos;
				model.AperturaDesde = DateTime.Now.AddDays(-1);
				model.AperturaHasta = DateTime.Now;
				model.Descripcion = string.Empty;

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

		public IActionResult CargarGrillaUsuariosEnSeccionDatosAdicionales(string inv_nro)
		{
			var model = new InventarioCargaGrillaUsuariosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _inventarioServicio.GetUsuariosEnInventario(inv_nro, TokenCookie);
				model.GrillaUsuarios = ObtenerGridCoreSmart<UsuarioEnInventarioDto>(lista);
				ListaUsuarioEnInventario = lista;
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

		public IActionResult AgregarUsuarioIndividual(string usu_id)
		{
			var model = new InventarioCargaGrillaUsuariosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (ListaUsuarios == null || ListaUsuarios.Count <= 0)
					return PartialView("_grillasAdicionalesUsuarios", model);

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
						inv_grupo = string.Empty,
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

		#region Metodos privados
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
			var lista = new List<ComboGenDto>
			{
				new ComboGenDto { Id = "1", Descripcion = "Conteo Simple" },
				new ComboGenDto { Id = "2", Descripcion = "Conteo Doble" },
				new ComboGenDto { Id = "3", Descripcion = "Conteo por Box" }
			};
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
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
		#endregion
	}
}
