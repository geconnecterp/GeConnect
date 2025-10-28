using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class AnticiposCargaController : AnticiposCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoAnticipoEmpleadoServicio _tipoAnticipoEmpleadoServicio;
		public AnticiposCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnticiposCargaController> logger,
										IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio, 
										ITipoAnticipoEmpleadoServicio tipoAnticipoEmpleadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
			_tipoAnticipoEmpleadoServicio = tipoAnticipoEmpleadoServicio;
		}

		public IActionResult Index()
		{
			var model = new CargaDeAnticiposModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGA DE ANTICIPOS";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(true);

				model.ListaTipo = ComboTipoAnticipoEmpleados();
				model.Concepto = string.Empty;
				model.porc_interes = 0;
				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>([]);

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

		[HttpPost]
		public JsonResult BuscarContrapartidas(string prefix)
		{
			var top = ProveedoresLista.Where(x => x.Cta_Lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Lista });
			return Json(tipos);
		}

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio, "PS");

			if (TipoAnticipoEmpleadoLista.Count == 0 || actualizar)
				ObtenerTiposAnticipoEmpleado(_tipoAnticipoEmpleadoServicio);
		}
		#endregion
	}
}
