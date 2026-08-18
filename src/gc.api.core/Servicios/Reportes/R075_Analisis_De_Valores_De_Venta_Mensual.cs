using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static gc.infraestructura.Helpers.GridHelper;

namespace gc.api.core.Servicios.Reportes
{
	public class R075_Analisis_De_Valores_De_Venta_Mensual : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R075_Analisis_De_Valores_De_Venta_Mensual(IUnitOfWork uow, IApiVentasServicio ventasSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_ventasSv = ventasSv;
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
				List<AnaValDeVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subtit;

				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{

				}).ToList();

				#endregion
				#region Scripts PDF
				#region instanciamos el pdf
				pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, false);

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
				var tituloBig = HelperPdf.FontTituloBigBoldPredeterminado();
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();

				#region Generación de Cabecera               

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, tituloBig, logo, _empresaGeco);

				// Convertir la tabla en un Phrase
				Phrase phrase = [tabla];

				// Crear el HeaderFooter con el Phrase que contiene la tabla
				HeaderFooter header = new(phrase, false)
				{
					Alignment = Element.ALIGN_TOP,
					BorderWidth = 0,
				};

				pdf.Header = header;
				#endregion

				pdf.Open();

				#region Lista 
				CargarRepoAnalisisDeValoresDeVentaMensual(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
				throw new NegocioException("Se produjo un error al intentar generar el Reporte de Extracto Bancario. Para mayores datos ver el log.");
			}
		}

		private List<AnaValDeVtaMesDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var sucursales = solicitud.Parametros.GetValueOrDefault("Sucursales", "")?.ToString() ?? null;
				var sucursalesTextos = solicitud.Parametros.GetValueOrDefault("SucursalesTextos", "")?.ToString() ?? null;
				var desde = solicitud.Parametros.GetValueOrDefault("Desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("Hasta", "").ToDateTime();
				var request = new AnaDeValDeVtaMesRequest()
				{
					adm_list = sucursales,
					desde = desde,
					hasta = hasta
				};
				var listaTemp = _ventasSv.ObtenerAnaDeValDeVtaMesLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis de Valores de Venta Mensual desde el {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";
				subtit = $"Sucursales: {sucursalesTextos}";
				return listaTemp;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<AnaValDeVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			string subtit;
			List<AnaValDeVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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

		#region funciones
		public static void CargarRepoAnalisisDeValoresDeVentaMensual(Document pdf, List<AnaValDeVtaMesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Título
			Paragraph tituloPar = new Paragraph("Análisis de Valores de Venta - Mensual", tituloBig);
			tituloPar.Alignment = Element.ALIGN_CENTER;
			tituloPar.SpacingAfter = 10f;
			pdf.Add(tituloPar);

			// Definición de columnas
			float[] widths = { 1f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			// Encabezados
			string[] headers = {
				"Mes",
				"Fact. + Cob.",
				"Cta. Cte.",
				"Cta. Cte. Dist.",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutuales",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			// Filas
			foreach (var item in registros)
			{
				tabla.AddCell(new PdfPCell(new Phrase($"{item.periodo}-{item.mes}", HelperPdf.FontChicoPredeterminado())));

				// Facturación + Cobranzas
				tabla.AddCell(CeldaSoloMonto(item.co_facturacion + item.co_cobranza, HelperPdf.FontChicoPredeterminado()));

				// Cta. Cte.
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_porc, item.co_ctacte, chico, HelperPdf.FontChicoPredeterminado()));

				// Cta. Cte. Dist.
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_dist_porc, item.co_ctacte_dist, chico, HelperPdf.FontChicoPredeterminado()));

				// Efectivo
				tabla.AddCell(CeldaPorcMonto(item.efectivos_porc, item.efectivos, chico, HelperPdf.FontChicoPredeterminado()));

				// Tarjetas
				tabla.AddCell(CeldaPorcMonto(item.tarjetas_porc, item.tarjetas, chico, HelperPdf.FontChicoPredeterminado()));

				// Transferencias
				tabla.AddCell(CeldaPorcMonto(item.bco_transf_porc, item.bco_transf, chico, HelperPdf.FontChicoPredeterminado()));

				// Mutuales
				tabla.AddCell(CeldaPorcMonto(item.mutuales_porc, item.mutuales, chico, HelperPdf.FontChicoPredeterminado()));

				// Vales
				tabla.AddCell(CeldaPorcMonto(item.vales_porc, item.vales, chico, HelperPdf.FontChicoPredeterminado()));

				// Otros
				tabla.AddCell(CeldaPorcMonto(item.otros_porc, item.otros, chico, HelperPdf.FontChicoPredeterminado()));
			}

			pdf.Add(tabla);
		}

		private static PdfPCell CeldaSoloMonto(decimal monto, Font normal)
		{
			PdfPCell c = new PdfPCell(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));

			c.HorizontalAlignment = Element.ALIGN_RIGHT;
			c.VerticalAlignment = Element.ALIGN_MIDDLE; // ← clave
			c.Padding = 4;

			return c;
		}

		private static PdfPCell CeldaPorcMonto(decimal porc, decimal monto, Font chico, Font normal)
		{
			PdfPCell cell = new PdfPCell();
			cell.Padding = 1;

			PdfPTable interno = new PdfPTable(2);
			interno.WidthPercentage = 100;
			interno.SetWidths(new float[] { 30f, 70f });

			// Colores
			BaseColor bg = porc > 0 ? new BaseColor(201, 228, 255) :
							porc < 0 ? new BaseColor(255, 224, 224) :
									   BaseColor.White;

			BaseColor fg = porc > 0 ? new BaseColor(0, 74, 133) :
							porc < 0 ? new BaseColor(161, 0, 0) :
									   new BaseColor(102, 102, 102);

			// Porcentaje
			PdfPCell cPorc = new PdfPCell(new Phrase(porc.ToString("0.##") + "%", chico));
			cPorc.BackgroundColor = bg;
			cPorc.Phrase.Font.Color = fg;
			cPorc.Border = Rectangle.NO_BORDER;
			cPorc.HorizontalAlignment = Element.ALIGN_LEFT;

			// Monto
			PdfPCell cMonto = new PdfPCell(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));
			cMonto.Border = Rectangle.NO_BORDER;
			cMonto.HorizontalAlignment = Element.ALIGN_RIGHT;

			interno.AddCell(cPorc);
			interno.AddCell(cMonto);

			cell.AddElement(interno);

			return cell;
		}
		#endregion
	}
}
