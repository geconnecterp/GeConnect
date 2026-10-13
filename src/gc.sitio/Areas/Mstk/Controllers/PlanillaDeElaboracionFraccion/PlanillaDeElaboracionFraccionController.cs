using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models.PlanillaDeElaboracionFraccion;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.PlanillaDeElaboracionFraccion
{
	[Area("Mstk")]
	public class PlanillaDeElaboracionFraccionController : PlanillaDeElaboracionFraccionControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IDepositoServicio _depositoServicio;
		public PlanillaDeElaboracionFraccionController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<PlanillaDeElaboracionFraccionController> logger,
													   IDepositoServicio depositoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_depositoServicio = depositoServicio;		
		}

		public IActionResult Index()
		{
			var model = new PlanillaDeElaboracionFraccionModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var titulo = "PLANILLA DE ELABORACION Y FRACCIÓN";
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

		public async Task<IActionResult> ObtenerBoxesDesdeDeposito(string depoId)
		{
			var model = new ListaBoxesModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (depoId != "0")
					model.ListaBoxes = CargarComboBoxes(depoId);
				else
				{
					List<DepositoInfoBoxDto> boxes = [];
					model.ListaBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				}
				return PartialView("_listaBoxes", model);
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

		#region Metodos Privados
		private void CargarDatosIniciales(PlanillaDeElaboracionFraccionModel model)
		{
			model.Nota = string.Empty;
			var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
			if (depositos != null && depositos.Count > 0)
				model.ListaDepositos = ComboDepositos(depositos);
			else
				model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.ListaBoxes = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.TablaProductosElaborados = ObtenerGridCoreSmart<ProductoModel>(new List<ProductoModel>());
			model.TablaProductosMateriaPrima = ObtenerGridCoreSmart<ProductoModel>(new List<ProductoModel>());
		}

		private SelectList ComboDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboBoxes(string depoId)
		{
			var adms = _depositoServicio.BuscarBoxPorDeposito(depoId, TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
