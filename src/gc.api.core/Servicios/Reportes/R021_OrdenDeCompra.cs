using Azure.Identity;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R021_OrdenDeCompra : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IApiProductoServicio _apiproductoServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R021_OrdenDeCompra(IUnitOfWork uow, IConsultaServicio consulta, IApiProductoServicio apiProductoServicio,
		   IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;

			_empresaGeco = empresa.Value;
			_titulos = ["Código", "Producto", "Cód. Prov.", "P. Lista", "Dto1", "Dto2", "Dto3", "Dto4", "Dto Pago", "BxP", "Cant", "Bonif.", "P. Costo", "Cat. Total", "Total",];
			_campos = ["Código", "Producto", "Cód. Prov.", "P. Lista", "Dto1", "Dto2", "Dto3", "Dto4", "Dto Pago", "BxP", "Cant", "Bonif.", "P. Costo", "Cat. Total", "Total",];
			_cuentaSv = consultaSv;
			_logger = logger;
			_apiproductoServicio = apiProductoServicio;
		}

		//TODO MARCE: Completar el reporte una vez que tengas certezas sobre los origenes de datos
		public string Generar(ReporteSolicitudDto solicitud)
		{
			float[] anchos;

			PdfWriter? writer = null;
			Document pdf;

			try
			{
				var ms = new MemoryStream();
				#region Obteniendo registros desde la base de datos
				//Cabecera
				string ctaId;
				string tit;
				string subTit;
				List<OrdenDeCompraDto> registros = ObtenerDatosCabecera(solicitud, out ctaId, out tit, out subTit);

				//Detalle
				List<OrdenDeCompraDetalleDto> registrosDetalle = ObtenerDatosDetalle(solicitud);

				if (registros == null || registros.Count == 0 || registrosDetalle == null || registrosDetalle.Count == 0)
				{
					throw new NegocioException($"No se encontraron registros para poder generar el reporte de Orden de Compra.");
				}

				//buscando datos del cliente
				var cta = _cuentaSv.GetCuentaComercialLista(ctaId, 'T');

				if (cta == null || cta.Count == 0)
				{
					throw new NegocioException($"No se encontraron datos del cliente {ctaId}.");
				}
				var cliente = cta[0];
				cliente.Monto = 0m;
				cliente.MontoEtiqueta = "";
				//COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
				//tit += cliente.Cta_Denominacion;
				solicitud.Titulo = tit;
				solicitud.SubTitulo = subTit;
				solicitud.Cuenta = cliente;


				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{
					#region Campos
					
					#endregion
				}).ToList();

				var agRet = regs.Select(x => new CertificadosDto()
				{
					// Campos del agente de retención
					
				}).First();

				var certi = regs.Select(x => new Certificado()
				{
					// Campos del certificado
					

				}).First();

				#endregion
				#region Scripts PDF
				#region instanciamos el pdf
				pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

				// Agregar el evento de pie de página
				writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

				var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

				#endregion
				//****=============================****/
				//****  CAMBIAR ANCHOS DE COLUMNAS ****
				//****=============================****/
				anchos = [70f, 30f];

				var chico = HelperPdf.FontChicoPredeterminado();
				var normal = HelperPdf.FontNormalPredeterminado();
				var normalBold = HelperPdf.FontNormalPredeterminado(true);
				var titulo = HelperPdf.FontTituloPredeterminado();
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();
				var subtituloBold = HelperPdf.FontSubtituloPredeterminado(true);

				#region Generación de Cabecera               

				PdfPTable tabla = GeneraCabeceraPDF2(solicitud, chico, titulo, logo, _empresaGeco);

				// Convertir la tabla en un Phrase
				Phrase phrase = new();
				phrase.Add(tabla);

				// Crear el HeaderFooter con el Phrase que contiene la tabla
				HeaderFooter header = new(phrase, false)
				{
					Alignment = Element.ALIGN_TOP,
					BorderWidth = 0,
				};

				pdf.Header = header;
				#endregion

				pdf.Open();

				#region Datos del Proveedor de la OC
				HelperPdf.CargarTablaDatosDeProveedorEnOrdenDeCompra(pdf, registros.First(), cliente, normalBold, normal, titulo);
				#endregion

				Chunk linebreak = new Chunk(new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 5));
				pdf.Add(linebreak);

				#region Datos del Detall de la OC
				HelperPdf.CargarTablaDatosDeDetalleEnOrdenDeCompra(pdf, registros.First(), registrosDetalle, chico, normalBold);
				#endregion

				#region Datos del certificado
				//HelperPdf.CargarTablaCertificadoIVADetalle(pdf, registros.Where(x => x.civa_base > 0).First(), subtitulo, subtituloBold, titulo);
				#endregion

				#region Firma
				//HelperPdf.CargarSeccionFirmaParaCertificadoDeRetencion(pdf, subtitulo, normal, titulo, false, 490, 380);
				#endregion

				//HelperPdf.CargarSeccionCopiaParaCertificadoDeRetencion(pdf, writer);

				pdf.Close();
				#endregion

				return Convert.ToBase64String(ms.ToArray());

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en R020");
				throw new NegocioException("Se produjo un error al intentar generar el reporte de Orden de Compras. Para mayores datos ver el log.");
			}
		}



		private List<OrdenDeCompraDto> ObtenerDatosCabecera(ReporteSolicitudDto solicitud, out string ctaId, out string titulo, out string subTitulo)
		{
			//Se obtienen los parámetros del reporte - Cabecera (Datos de la cuenta)
			ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
			string cmptId = solicitud.Parametros.GetValueOrDefault("oc_compte", "").ToString();
			titulo = $"Orden de Compra N° {cmptId}";
			var lista = _apiproductoServicio.ObtenerOrdenDeCompraPorOcCompte(cmptId);
			subTitulo = $"Estado: {lista.FirstOrDefault().Oce_Desc}";
			return lista;
		}
		private List<OrdenDeCompraDetalleDto> ObtenerDatosDetalle(ReporteSolicitudDto solicitud)
		{
			//Se obtienen los parámetros del reporte - Detalle (Datos del detalle de la Orden de Compra seleccionada)
			string cmptId = solicitud.Parametros.GetValueOrDefault("oc_compte", "").ToString();
			return _apiproductoServicio.CargarDetalleDeOC(cmptId);
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string ctaId;
			string tit;
			string subTit;
			List<OrdenDeCompraDto> registros = ObtenerDatosCabecera(solicitud, out ctaId, out tit, out subTit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{
				//TODO MARCE: Agregar datos aca para generar el TXT
			}).ToList();


			#endregion

			return GeneraTXT(regs, _campos);
		}

		public string GenerarXls(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string ctaId;
			string tit;
			string subTit;
			List<OrdenDeCompraDto> registros = ObtenerDatosCabecera(solicitud, out ctaId, out tit, out subTit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{
				//TODO MARCE: Agregar datos aca para generar el XLS
			}).ToList();

			#endregion

			return GeneraFileXLS(regs, _titulos, _campos);
		}
	}
}
