using Azure.Identity;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
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

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R021_OrdenDeCompra(IUnitOfWork uow, IConsultaServicio consulta,
		   IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;

			_empresaGeco = empresa.Value;
			_titulos = ["Código", "Producto", "Cód. Prov.", "P. Lista", "Dto1", "Dto2", "Dto3", "Dto4", "Dto Pago", "BxP", "Cant", "Bonif.", "P. Costo", "Cat. Total", "Total",];
			_campos = ["Código", "Producto", "Cód. Prov.", "P. Lista", "Dto1", "Dto2", "Dto3", "Dto4", "Dto Pago", "BxP", "Cant", "Bonif.", "P. Costo", "Cat. Total", "Total",];
			_cuentaSv = consultaSv;
			_logger = logger;
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
				string ctaId;
				string tit;
				List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

				if (registros == null || registros.Count == 0)
				{
					throw new NegocioException($"No se encontraron registros para poder generar el Certificado de Retención de IVA.");
				}

				//var importe = registros.Sum(x => x.Cc_importe);

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
				solicitud.Cuenta = cliente;


				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{
					#region Campos
					civaNro = x.civa_nro,
					ctaId = x.cta_id,
					civaCuit = x.civa_cuit,
					civaRazSoc = x.civa_raz_soc,
					civaDomicilio = x.civa_domicilio,
					civaFecha = x.civa_fecha,
					civaBase = x.civa_base,
					civaReten = x.civa_reten,
					civaEstado = x.civa_estado,
					opCompte = x.op_compte,
					civaActu = x.civa_actu,
					civaImpreso = x.civa_impreso,
					empRazonSocial = x.emp_razon_social,
					empCuit = x.emp_cuit,
					empIbNro = x.emp_ib_nro,
					empDomicilio = x.emp_domicilio,
					#endregion
				}).ToList();

				var agRet = regs.Select(x => new CertificadosDto()
				{
					// Campos del agente de retención
					emp_cuit = x.empCuit,
					emp_razon_social = x.empRazonSocial,
					emp_domicilio = x.empDomicilio,
					emp_ib_nro = x.empIbNro,
				}).First();

				var certi = regs.Select(x => new Certificado()
				{
					// Campos del certificado
					id = x.civaNro,
					fecha = x.civaFecha,

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

				PdfPTable tabla = GeneraCabeceraPdf3C(solicitud, chico, titulo, logo, _empresaGeco);

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

				#region Datos del Agente de Retencion
				HelperPdf.CargarTablaAgenteDeRetencion1Col(pdf, agRet, normal, normalBold, titulo, false);
				#endregion

				Chunk linebreak = new Chunk(new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 5));
				pdf.Add(linebreak);

				#region Datos datos/titulo del certificado
				HelperPdf.CargarTablaCertificado(pdf, certi, normal, normalBold, titulo);
				#endregion

				#region Datos del certificado
				HelperPdf.CargarTablaCertificadoIVADetalle(pdf, registros.Where(x => x.civa_base > 0).First(), subtitulo, subtituloBold, titulo);
				#endregion

				#region Firma
				HelperPdf.CargarSeccionFirmaParaCertificadoDeRetencion(pdf, subtitulo, normal, titulo, false, 490, 380);
				#endregion

				HelperPdf.CargarSeccionCopiaParaCertificadoDeRetencion(pdf, writer);

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
				throw new NegocioException("Se produjo un error al intentar generar el Certificado de Retencion de IVA. Para mayores datos ver el log.");
			}
		}



		private List<CertRetenIVADto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
		{
			//Se obtienen los parámetros del reporte
			ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
			string cmptId = solicitud.Parametros.GetValueOrDefault("op_compte", "").ToString();
			titulo = $"Certificado de Retención de IVA";
			return _consultaServicio.ConsultaCertRetenIVA(cmptId);

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string ctaId;
			string tit;
			List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{
				civaNro = x.civa_nro,
				ctaId = x.cta_id,
				civaCuit = x.civa_cuit,
				civaRazSoc = x.civa_raz_soc,
				civaDomicilio = x.civa_domicilio,
				civaFecha = x.civa_fecha,
				civaBase = x.civa_base,
				civaReten = x.civa_reten,
				civaEstado = x.civa_estado,
				opCompte = x.op_compte,
				civaActu = x.civa_actu,
				civaImpreso = x.civa_impreso,
				empRazonSocial = x.emp_razon_social,
				empCuit = x.emp_cuit,
				empIbNro = x.emp_ib_nro,
				empDomicilio = x.emp_domicilio,
			}).ToList();


			#endregion

			return GeneraTXT(regs, _campos);
		}

		public string GenerarXls(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string ctaId;
			string tit;
			List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{
				civaNro = x.civa_nro,
				ctaId = x.cta_id,
				civaCuit = x.civa_cuit,
				civaRazSoc = x.civa_raz_soc,
				civaDomicilio = x.civa_domicilio,
				civaFecha = x.civa_fecha,
				civaBase = x.civa_base,
				civaReten = x.civa_reten,
				civaEstado = x.civa_estado,
				opCompte = x.op_compte,
				civaActu = x.civa_actu,
				civaImpreso = x.civa_impreso,
				empRazonSocial = x.emp_razon_social,
				empCuit = x.emp_cuit,
				empIbNro = x.emp_ib_nro,
				empDomicilio = x.emp_domicilio,
			}).ToList();

			#endregion

			return GeneraFileXLS(regs, _titulos, _campos);
		}
	}
}
