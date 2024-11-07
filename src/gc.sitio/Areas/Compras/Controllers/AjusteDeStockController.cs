using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class AjusteDeStockController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly ILogger<CompraController> _logger;

		public AjusteDeStockController(IProductoServicio productoServicio, IDepositoServicio depositoServicio, 
									   ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_depositoServicio = depositoServicio;
			_productoServicio = productoServicio;	
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> AjustesDeStock()
		{
			AjusteDeStockDto model = new AjusteDeStockDto();
			List<DepositoInfoBoxDto> boxes = [];
			try
			{
				model.ComboDepositos = CargarComboDepositos();
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				model.ComboMotivos = CargarComboTiposDeAjusteDeStock();
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

		#region Métodos Privados
		private SelectList CargarComboDepositos()
		{
			var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboTiposDeAjusteDeStock()
		{
			var adms = _productoServicio.ObtenerTipoDeAjusteDeStock(TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = $"{x.at_id}#{x.at_tipo}", Descripcion = x.at_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
