using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R097_Impresion_De_Etiquetas_UL : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R097_Impresion_De_Etiquetas_UL(IUnitOfWork uow, IApiProductoServicio apiProdSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiProdSv = apiProdSv;
		}

		public string Generar(ReporteSolicitudDto solicitud)
		{
			float[] anchos;

			PdfWriter? writer = null;
			Document pdf;

			try
			{
				var ms = new MemoryStream();
				#region Obteniendo registros desde la base de datos
				string tit;
				string subtit;
				string filtrosString;
				List<ImprimirULDto> registros = ObtenerDatos(solicitud, out tit);

				solicitud.Titulo = tit;

				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{

				}).ToList();

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

				var chicoplus = HelperPdf.FontSuperChicoPredeterminado();
				var chico = HelperPdf.FontChicoPredeterminado();
				var chicoBold = HelperPdf.FontChicoPredeterminado(true);
				var normal = HelperPdf.FontNormalPredeterminado();
				var normalBold = HelperPdf.FontNormalPredeterminado(true);
				var titulo = HelperPdf.FontTituloPredeterminado();
				var tituloBig = HelperPdf.FontTituloBigBoldPredeterminado();
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();

				#region Generación de Cabecera               

				//PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, tituloBig, logo, _empresaGeco);

				// Convertir la tabla en un Phrase
				//Phrase phrase = [tabla];

				// Crear el HeaderFooter con el Phrase que contiene la tabla
				//HeaderFooter header = new(phrase, false)
				//{
				//	Alignment = Element.ALIGN_TOP,
				//	BorderWidth = 0,
				//};

				//pdf.Header = header;
				#endregion

				pdf.Open();

				#region Armado de Reporte
				CargarRepoEtiquetasUL(pdf, registros, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig, writer);
				#endregion

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
				_logger.LogError(ex, "Error en R031");
				throw new NegocioException("Se produjo un error al intentar generar el Reporte Analítico de Operaciones. Para mayores datos ver el log.");
			}
		}

		#region Funciones de generacion de secciones de reportes
		public static void CargarRepoEtiquetasUL(Document pdf, List<ImprimirULDto> registros, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig, PdfWriter writer)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para imprimir.", normal));
				return;
			}

			PdfPTable tabla = new PdfPTable(2);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 1f, 1f });

			PdfContentByte cb = writer.DirectContent;

			int contador = 0;

			foreach (var item in registros)
			{
				contador++;

				PdfPTable etiqueta = new PdfPTable(1);
				etiqueta.WidthPercentage = 100;

				etiqueta.AddCell(new PdfPCell(new Phrase("TIPO: " + item.tipo, normalBold))
				{
					Border = Rectangle.NO_BORDER,
					PaddingBottom = 4
				});

				etiqueta.AddCell(new PdfPCell(new Phrase("TIPO ID: " + item.tipo_id, normal))
				{
					Border = Rectangle.NO_BORDER,
					PaddingBottom = 4
				});

				etiqueta.AddCell(new PdfPCell(new Phrase("Motivo: " + item.motivo, normal))
				{
					Border = Rectangle.NO_BORDER,
					PaddingBottom = 4
				});

				etiqueta.AddCell(new PdfPCell(new Phrase("UL_ID: " + item.ul_id, normalBold))
				{
					Border = Rectangle.NO_BORDER,
					PaddingBottom = 6
				});

				// ============================
				// BARCODE CODE39
				// ============================
				Barcode39 barcode = new Barcode39();
				barcode.Code = "*" + item.ul_id + "*";   // obligatorio para scanner
				barcode.StartStopText = false;
				barcode.GenerateChecksum = false;

				// Crear imagen del barcode
				Image barcodeImage = barcode.CreateImageWithBarcode(cb, BaseColor.Black, BaseColor.Black);

				// Ajustar tamaño (proporción ideal para etiquetas A4)
				barcodeImage.ScaleToFit(160f, 40f);   // ancho máx 160px, alto máx 40px

				// Centrar y agregar padding
				PdfPCell celdaBarcode = new PdfPCell(barcodeImage)
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingTop = 6,
					PaddingBottom = 6
				};

				etiqueta.AddCell(celdaBarcode);


				etiqueta.AddCell(new PdfPCell(new Phrase("Fecha impresión: " +
					item.hoy.ToString("dd/MM/yyyy HH:mm"), chico))
				{
					Border = Rectangle.NO_BORDER,
					PaddingTop = 6
				});

				PdfPCell celdaEtiqueta = new PdfPCell(etiqueta)
				{
					Padding = 10,
					Border = Rectangle.BOX,
					BorderWidth = 0.5f
				};

				tabla.AddCell(celdaEtiqueta);

				if (contador == 14)
					break;
			}

			if (contador % 2 != 0)
				tabla.AddCell(new PdfPCell(new Phrase("")));

			pdf.Add(tabla);
		}


		#endregion

		private List<ImprimirULDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			try
			{
				var ret = new List<ImprimirULDto>();
				var tipo = solicitud.Parametros.GetValueOrDefault("tipo", "")?.ToString() ?? null;
				var ul_id = solicitud.Parametros.GetValueOrDefault("rpId", "")?.ToString() ?? null;
				
				ret = _apiProdSv.RPRULImprime(tipo, ul_id);
				titulo = string.Empty;
				return ret;
			}
			catch (Exception)
			{
				titulo = string.Empty;
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<ImprimirULDto> registros = ObtenerDatos(solicitud, out tit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{

			}).ToList();


			#endregion

			return GeneraTXT(regs, _campos);
		}

		public string GenerarXls(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<ImprimirULDto> registros = ObtenerDatos(solicitud, out tit);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{

			}).ToList();

			#endregion

			return GeneraFileXLS(regs, _titulos, _campos);
		}
	}
}
