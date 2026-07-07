using gc.api.Controllers.Users;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Dtos.Ventas;
using log4net.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Consultas
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultaCCController : ControllerBase
    {
        private readonly ILogger<ConsultaCCController> _logger;
        private readonly IHttpContextAccessor _context;
        private readonly IConsultaServicio _consSv;
        private readonly IUriService _uriService;

        public ConsultaCCController(ILogger<ConsultaCCController> logger, IHttpContextAccessor accessor, 
            IConsultaServicio consulta, IUriService uriService)
        {
            _logger = logger;
            _context = accessor;
            _consSv = consulta;
            _uriService = uriService;
        }

        [HttpGet]
        public IActionResult ConsultarCuentaCorriente(string ctaId, long fechaD, string userId,int pagina,int registros)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fechaD == 0)
            {
                return BadRequest("No se ha especificado el periodo DESDE");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var fd = new DateTime(fechaD);
            ConsCtaCteDto reg = new ConsCtaCteDto();
            var regs = _consSv.ConsultarCuentaCorriente(ctaId, fd, userId,pagina, registros);
            if (regs.Count > 0)
            {
                reg = regs[0];
            }

            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = registros,
                CurrentPage = pagina,
                TotalPages = reg.Total_paginas,
                HasNextPage = pagina < reg.Total_paginas,
                HasPreviousPage = pagina> 1,
                NextPageUrl = _uriService.GetPostPaginationUri(new QueryFilters(), Url.RouteUrl(nameof(ConsultarCuentaCorriente)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(new QueryFilters(), Url.RouteUrl(nameof(ConsultarCuentaCorriente)) ?? "").ToString(),

            };
            var response = new ApiResponse<IEnumerable<ConsCtaCteDto>>(regs)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaVencimientoComprobantesNoImputados(string ctaId, long fechaD, long fechaH, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fechaD == 0)
            {
                return BadRequest("No se ha especificado la fecha DESDE");
            }
            if (fechaH == 0)
            {
                return BadRequest("No se ha especificado la fecha hasta");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var fd = new DateTime(fechaD);
            var fh = new DateTime(fechaH);

            var regs = _consSv.ConsultaVencimientoComprobantesNoImputados(ctaId, fd, fh, userId);

            return Ok(new ApiResponse<List<ConsVtoDto>>(regs));
        }
        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (meses < 1 || meses >60)
            {
                return BadRequest("No se ha especificado, correctamente, la cantidad meses. Se pueden especificar hasta 60 meses.");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }          

            var regs = _consSv.ConsultaComprobantesMeses(ctaId, meses,relCuit, userId);
            return Ok(new ApiResponse<List<ConsCompTotDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (mes.Length != 6 && mes.ToIntOrNull()==null)
            {
                return BadRequest("No se ha especificado el mes");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            

            var regs = _consSv.ConsultaComprobantesMesDetalle(ctaId, mes,relCuit, userId);
            return Ok(new ApiResponse<List<ConsCompDetDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaOrdenesDePagoProveedor(string ctaId, long fecD,long fecH, string tipoOP, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fecH<fecD)
            {
                return BadRequest("No se ha especificado correctamente el intervalo de tiempo desde hasta");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var fd = new DateTime(fecD);
            var fh = new DateTime(fecH);

            var regs = _consSv.ConsultaOrdenesDePagoProveedor(ctaId, fd, fh, tipoOP, userId);
            return Ok(new ApiResponse<List<ConsOrdPagosDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaOrdenesDePagoProveedorDetalle(string cmptId)
        {
            if (string.IsNullOrEmpty(cmptId))
            {
                return BadRequest("No se recepcionó ninguna comprobante");
            }
          

            var regs = _consSv.ConsultaOrdenesDePagoProveedorDetalle(cmptId);
            return Ok(new ApiResponse<List<ConsOrdPagosDetDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaRecepcionProveedor(string ctaId, long fecD, long fecH, string admId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fecH < fecD)
            {
                return BadRequest("No se ha especificado correctamente el intervalo de tiempo desde hasta");
            }
    
            var fd = new DateTime(fecD);
            var fh = new DateTime(fecH);

            var regs = _consSv.ConsultaRecepcionProveedor(ctaId, fd, fh, admId);
            return Ok(new ApiResponse<List<ConsRecepcionProveedorDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaRecepcionProveedorDetalle(string cmptId)
        {
            if (string.IsNullOrEmpty(cmptId))
            {
                return BadRequest("No se recepcionó ninguna comprobante");
            }


            var regs = _consSv.ConsultaRecepcionProveedorDetalle(cmptId);
            return Ok(new ApiResponse<List<ConsRecepcionProveedorDetalleDto>>(regs));
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaOrdPagoDetExtend(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}
			
            var res = _consSv.ConsultaOrdenDePagoProveedor(opCompte);
			return Ok(new ApiResponse<List<ConsOrdPagoDetExtendDto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenIB(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenIB(opCompte);
			return Ok(new ApiResponse<List<CertRetenIBDto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenIBFromList(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenIBFromList(opCompte);
			return Ok(new ApiResponse<List<CertRetenIBDto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenIVA(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenIVA(opCompte);
			return Ok(new ApiResponse<List<CertRetenIVADto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenIVAFromList(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenIVAFromList(opCompte);
			return Ok(new ApiResponse<List<CertRetenIVADto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenGAN(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenGA(opCompte);
			return Ok(new ApiResponse<List<CertRetenGananDto>>(res));
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult ConsultaCertRetenGANFromList(string opCompte)
		{
			if (string.IsNullOrEmpty(opCompte))
			{
				return BadRequest("No se recepcionó ningún comprobante");
			}

			var res = _consSv.ConsultaCertRetenGAFromList(opCompte);
			return Ok(new ApiResponse<List<CertRetenGananDto>>(res));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<VencimientoListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultarVencimientosPorTipo(ConsultarVencimientosRequest request)
		{
			VencimientoListaDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultarVencimientosPorTipo(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarVencimientosPorTipo)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarVencimientosPorTipo)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<VencimientoListaDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CertificadoListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultarCertificadosNRNP(ConsultarCertificadosRequest request)
		{
			CertificadoListaDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultarCertificadosNRNP(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarVencimientosPorTipo)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarVencimientosPorTipo)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<CertificadoListaDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoStkDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultarProductoStk(ConsultarStockRequest request)
		{
			ProductoStkDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultarProductoStk(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStk)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStk)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<ProductoStkDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoStkDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultarProductoStkValor(ConsultarStockValorizadoRequest request)
		{
			ProductoStkDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultarProductoStkValor(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStk)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStk)) ?? "").ToString(),
			};

			var response = new ApiResponse<IEnumerable<ProductoStkDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoStkCompensadoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultarProductoStkCompensado(ConsultarStockCompensadoRequest request)
		{
			ProductoStkCompensadoDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultarProductoStkCompensado(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStkCompensado)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(ConsultarProductoStkCompensado)) ?? "").ToString(),
			};

			var response = new ApiResponse<IEnumerable<ProductoStkCompensadoDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<MovimientoListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConsultaMovimientoLista(BuscarMovDeCuentaDirectaRequest request)
		{
			ApiResponse<List<MovimientoListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.ConsultaMovimientoLista(request);

			response = new ApiResponse<List<MovimientoListaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SaldoDetalleDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarSaldoDetalleCtaDistribuidora(BuscarSaldoDetalleRequest request)
		{
			ApiResponse<List<SaldoDetalleDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarSaldoDetalleCtaDistribuidora(request);

			response = new ApiResponse<List<SaldoDetalleDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SaldoResumenDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarSaldoResumenCtaDistribuidora(BuscarSaldoDetalleRequest request)
		{
			ApiResponse<List<SaldoResumenDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarSaldoResumenCtaDistribuidora(request);

			response = new ApiResponse<List<SaldoResumenDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ComisionesDeVendedoresDetalleDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarComisionDeVendedorDetalle(ComisionesDeVendedoresRequest request)
		{
			ApiResponse<List<ComisionesDeVendedoresDetalleDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarComisionDeVendedorDetalle(request);
			response = new ApiResponse<List<ComisionesDeVendedoresDetalleDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ComisionesDeVendedoresResumenDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarComisionDeVendedorResumen(ComisionesDeVendedoresRequest request)
		{
			ApiResponse<List<ComisionesDeVendedoresResumenDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarComisionDeVendedorResumen(request);	
			response = new ApiResponse<List<ComisionesDeVendedoresResumenDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ComisionesDeRepartidoresDetalleDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarComisionDeRepartidorDetalle(ComisionesDeRepartidoresRequest request)
		{
			ApiResponse<List<ComisionesDeRepartidoresDetalleDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarComisionDeRepartidorDetalle(request);
			response = new ApiResponse<List<ComisionesDeRepartidoresDetalleDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ComisionesDeRepartidoresResumenDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarComisionDeRepartidorResumen(ComisionesDeRepartidoresRequest request)
		{
			ApiResponse<List<ComisionesDeRepartidoresResumenDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _consSv.BuscarComisionDeRepartidorResumen(request);
			response = new ApiResponse<List<ComisionesDeRepartidoresResumenDto>>(res);

			return Ok(response);
		}
	}
}
