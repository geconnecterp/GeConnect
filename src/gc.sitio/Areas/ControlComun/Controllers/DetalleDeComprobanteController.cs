using AutoMapper;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.ControlComun.Models.DetalleDeComprobante;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class DetalleDeComprobanteController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IConsultasServicio _consultasServicio;
		private readonly IMapper _mapper;
		public DetalleDeComprobanteController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<DetalleDeComprobanteController> logger,
											  IConsultasServicio consultasServicio, IMapper mapper) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_consultasServicio = consultasServicio;
			_mapper = mapper;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AbrirComponente(DetalleDeComprobanteRequest request)
		{
			RespuestaGenerica<EntidadBase> response = new();
			var model = new DetalleDeCompteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var cab = _consultasServicio.BuscarDetalleDeComprobanteCab(request, TokenCookie);
				if (cab == null || cab.Count <= 0)
				{
					response.Mensaje = $"No se encontró comprobante. Tipo: {request.tco_id} Comprobante: {request.cm_compte} Mov: {request.dia_movi}";
					response.Ok = false;
					response.EsWarn = true;
					response.EsError = false;
					return PartialView("_gridMensaje", response);
				}
				var cabModel = new DetalleDeCompteCabModel();
				MapperCab(cab.FirstOrDefault(), cabModel);
				var iva = _consultasServicio.BuscarDetalleDeComprobanteIva(request, TokenCookie);
				var ivalModel = new DetalleDeCompteIvaModel();
				MapperIva(iva.FirstOrDefault(), ivalModel);
				var per = _consultasServicio.BuscarDetalleDeComprobantePer(request, TokenCookie);
				var perModel = new DetalleDeComptePerModel();
				MapperPer(per.FirstOrDefault(), perModel);

				model.Cab = cabModel;
				model.Iva = ivalModel;
				model.Per = perModel;

				return View("~/areas/ControlComun/views/SeleccionDeValores/_index.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{
				string msg = "Error en la obtención de la configuración para el componente.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		private void MapperCab(DetalleDeComprobanteCabDto dto, DetalleDeCompteCabModel model)
		{
			if (dto == null)
				return;

			_mapper.Map(dto, model);
		}

		private void MapperPer(DetalleDeComprobantePerDto dto, DetalleDeComptePerModel model)
		{
			if (dto == null)
				return;

			_mapper.Map(dto, model);
		}

		private void MapperIva(DetalleDeComprobanteIvaDto dto, DetalleDeCompteIvaModel model)
		{
			if (dto == null)
				return;

			_mapper.Map(dto, model);
		}
	}
	public class ComprobantesProfile : Profile
	{
		public ComprobantesProfile()
		{
			CreateMap<DetalleDeCompteCabModel, DetalleDeComprobanteCabDto>();
			CreateMap<DetalleDeComprobanteCabDto, DetalleDeCompteCabModel>();

			// PER
			CreateMap<DetalleDeComptePerModel, DetalleDeComprobantePerDto>()
				.ForMember(dest => dest.@base, opt => opt.MapFrom(src => src.@base));

			CreateMap<DetalleDeComprobantePerDto, DetalleDeComptePerModel>()
				.ForMember(dest => dest.@base, opt => opt.MapFrom(src => src.@base));

			// IVA
			CreateMap<DetalleDeCompteIvaModel, DetalleDeComprobanteIvaDto>();
			CreateMap<DetalleDeComprobanteIvaDto, DetalleDeCompteIvaModel>();
		}
	}
}
