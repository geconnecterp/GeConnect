using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
	public class R087_Saldo_Cta_Distr_Resumen : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R087_Saldo_Cta_Distr_Resumen(IUnitOfWork uow, IConsultaServicio consSrv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_consSrv = consSrv;
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
				List<MovimientoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				var chicoBold = HelperPdf.FontChicoPredeterminado(true);
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

				#region Armado de Reporte
				CargarRepoMovimientoCtaDirecta(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarRepoMovimientoCtaDirecta(Document pdf, List<MovimientoListaDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			var grupos = registros
		.GroupBy(r => r.ctag_id)
		.OrderBy(g => g.Key);

			decimal totalGeneral = 0m;
			var cultura = new CultureInfo("es-AR");

			// Definimos un ancho más generoso para la columna Importe
			float[] widths = new float[] { 12f, 8f, 8f, 12f, 22f, 12f, 14f, 12f }; // última columna más ancha

			foreach (var grupo in grupos)
			{
				string ctagId = grupo.Key;
				string ctagDen = grupo.First().ctag_denominacion;

				decimal subtotal = grupo.Sum(x => x.cc_importe);
				totalGeneral += subtotal;

				// ============================
				// ENCABEZADO DEL GRUPO (Cuenta sin relieve + ID y Denominación con relieve)
				// ============================

				PdfPTable tblEnc = new PdfPTable(3);   // 3 columnas: Cuenta | ID | Denominación
				tblEnc.WidthPercentage = 100;
				tblEnc.SetWidths(new float[] { 12f, 20f, 68f }); // proporciones equilibradas

				// --- Celda "Cuenta" (SIN relieve) ---
				PdfPCell celCuenta = new PdfPCell(new Phrase("Cuenta", normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingBottom = 6f
				};

				// --- Recuadro 3D para ctag_id ---
				PdfPCell celId = new PdfPCell(new Phrase(ctagId, normalBold))
				{
					Padding = 6f,
					BackgroundColor = new BaseColor(245, 245, 245),
					BorderWidthLeft = 1.8f,
					BorderWidthTop = 1.8f,
					BorderWidthRight = 0.8f,
					BorderWidthBottom = 0.8f,
					BorderColorLeft = new BaseColor(255, 255, 255),
					BorderColorTop = new BaseColor(255, 255, 255),
					BorderColorRight = new BaseColor(120, 120, 120),
					BorderColorBottom = new BaseColor(120, 120, 120),
					HorizontalAlignment = Element.ALIGN_CENTER
				};

				// --- Recuadro 3D para ctag_denominacion ---
				PdfPCell celDen = new PdfPCell(new Phrase(ctagDen, normalBold))
				{
					Padding = 6f,
					BackgroundColor = new BaseColor(245, 245, 245),
					BorderWidthLeft = 1.8f,
					BorderWidthTop = 1.8f,
					BorderWidthRight = 0.8f,
					BorderWidthBottom = 0.8f,
					BorderColorLeft = new BaseColor(255, 255, 255),
					BorderColorTop = new BaseColor(255, 255, 255),
					BorderColorRight = new BaseColor(120, 120, 120),
					BorderColorBottom = new BaseColor(120, 120, 120),
					HorizontalAlignment = Element.ALIGN_LEFT
				};

				tblEnc.AddCell(celCuenta);
				tblEnc.AddCell(celId);
				tblEnc.AddCell(celDen);

				pdf.Add(tblEnc);


				// Tabla detalle
				PdfPTable tbl = new PdfPTable(8);
				tbl.WidthPercentage = 100;
				tbl.SetWidths(widths);

				AgregarHeader(tbl, "Orden Pag./Liq.", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Fecha", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Tipo Compte.", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Comprobante", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Razón Social", normalBold, Element.ALIGN_LEFT);
				AgregarHeader(tbl, "CUIT", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Motivo", normalBold, Element.ALIGN_LEFT);
				AgregarHeader(tbl, "Importe", normalBold, Element.ALIGN_RIGHT);

				foreach (var r in grupo)
				{
					AgregarCelda(tbl, $"{r.origen} {r.op_compte}", normal, Element.ALIGN_CENTER);
					AgregarCelda(tbl, r.op_fecha.ToString("dd/MM/yyyy"), normal, Element.ALIGN_CENTER);
					AgregarCelda(tbl, r.tco_id, normal, Element.ALIGN_CENTER);
					AgregarCelda(tbl, r.cm_compte, normal, Element.ALIGN_CENTER);
					AgregarCelda(tbl, r.cm_nombre, normal, Element.ALIGN_LEFT);
					AgregarCelda(tbl, r.cm_cuit, normal, Element.ALIGN_CENTER);
					AgregarCelda(tbl, r.ctag_motivo, normal, Element.ALIGN_LEFT);
					AgregarCelda(tbl, r.cc_importe.ToString("#,##0.00", cultura), normal, Element.ALIGN_RIGHT);
				}

				pdf.Add(tbl);

				// Subtotal
				PdfPTable tblSub = new PdfPTable(8);
				tblSub.WidthPercentage = 100;
				tblSub.SetWidths(widths);

				PdfPCell celLbl = new PdfPCell(new Phrase($"Subtotal Cuenta {ctagId} - {ctagDen}", normalBold))
				{
					Colspan = 7,
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT
				};

				PdfPCell celVal = new PdfPCell(new Phrase(subtotal.ToString("#,##0.00", cultura), normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT
				};

				tblSub.AddCell(celLbl);
				tblSub.AddCell(celVal);
				pdf.Add(tblSub);

				pdf.Add(new Paragraph(" ", chico));
			}

			// TOTAL GENERAL
			PdfPTable tblTot = new PdfPTable(8);
			tblTot.WidthPercentage = 100;
			tblTot.SetWidths(widths);

			PdfPCell celLblTot = new PdfPCell(new Phrase("TOTAL", tituloBig))
			{
				Colspan = 7,
				Border = Rectangle.TOP_BORDER,
				BorderWidthTop = 2f,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 6f
			};

			PdfPCell celValTot = new PdfPCell(new Phrase(totalGeneral.ToString("#,##0.00", cultura), tituloBig))
			{
				Border = Rectangle.TOP_BORDER,
				BorderWidthTop = 2f,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 6f
			};

			tblTot.AddCell(celLblTot);
			tblTot.AddCell(celValTot);
			pdf.Add(tblTot);
		}

		#endregion
		private static void AgregarHeader(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell cel = new PdfPCell(new Phrase(texto, font));
			cel.HorizontalAlignment = align;
			cel.VerticalAlignment = Element.ALIGN_MIDDLE;
			cel.BackgroundColor = new BaseColor(230, 230, 230);
			cel.BorderWidth = 0.75f;
			tbl.AddCell(cel);
		}

		private static void AgregarCelda(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell cel = new PdfPCell(new Phrase(texto, font));
			cel.HorizontalAlignment = align;
			cel.VerticalAlignment = Element.ALIGN_MIDDLE;
			cel.BorderWidth = 0.5f;
			tbl.AddCell(cel);
		}


		private List<MovimientoListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var ret = new List<MovimientoListaDto>();
				var ctag_list_temp = solicitud.Parametros.GetValueOrDefault("ctag_list", "")?.ToString() ?? null;
				List<string> lista = ctag_list_temp.Split(',').ToList();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				titulo = $"Consulta de Cuenta de Gastos";
				ret = _consSrv.ConsultaMovimientoLista(new BuscarMovDeCuentaDirectaRequest() { 
					desde=desde,
					hasta=hasta,
					ctag_list= lista
				});
				subtit = $"Desde: {desde:dd/MM/yyyy} Hasta: {hasta:dd/MM/yyyy}";
				return ret;
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
			List<MovimientoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<MovimientoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
