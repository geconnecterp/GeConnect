using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R103_Planilla_Elaboracion_Y_Fraccion : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _prdSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R103_Planilla_Elaboracion_Y_Fraccion(IUnitOfWork uow, IApiProductoServicio prdSrv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_prdSrv = prdSrv;
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
				List<PlanillaElabYFraccionDto> registros = ObtenerDatos(solicitud, out tit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = "";

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
				var tituloNoBold = HelperPdf.FontTituloPredeterminado(false);
				var tituloBig = HelperPdf.FontTituloBigBoldPredeterminado();
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();

				#region Generación de Cabecera               

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, tituloNoBold, titulo, tituloBig, logo, _empresaGeco, registros.First());

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
				CargarRepoDePlanillaDeElabYFraccion(pdf, registros, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		private PdfPTable GeneraCabeceraPDF2_NoFecha(ReporteSolicitudDto solicitud, Font chico, Font normal, Font titulo, Font tituloBig, Image? logo, EmpresaGeco _empresaGeco, PlanillaElabYFraccionDto reg, int titHorAlign = 1, int subtitHorAlign = 1)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 20f, 70f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", tituloBig));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Responsabilidad} Ini.Act:{_empresaGeco.InicioActividades.ToShortDateString()}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} IB:{_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Direccion}, {_empresaGeco.Localidad}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// ============================
			// COLUMNA 3 — TÍTULO + SUCURSAL + DEPÓSITO + FECHA
			// ============================
			PdfPTable subTablaC3 = new PdfPTable(3);
			subTablaC3.WidthPercentage = 100;
			subTablaC3.SetWidths(new float[] { 30f, 40f, 30f });
			// Col 1: títulos (Sucursal / Depósito)
			// Col 2: valores (adm_nombre / depo_nombre)
			// Col 3: fecha (solo en fila Depósito)

			// --- TÍTULO PRINCIPAL ---
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = titHorAlign,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingBottom = 8f,
				Colspan = 3
			};
			subTablaC3.AddCell(celdaTitulo);

			// --- FILA SUCURSAL ---
			subTablaC3.AddCell(new PdfPCell(new Phrase("Sucursal:", normal))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			});

			subTablaC3.AddCell(new PdfPCell(new Phrase(reg.adm_nombre, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			});

			// celda vacía para esta fila (fecha va solo en la fila Depósito)
			subTablaC3.AddCell(new PdfPCell(new Phrase("", titulo))
			{
				Border = Rectangle.NO_BORDER
			});

			// --- FILA DEPÓSITO + FECHA ---
			subTablaC3.AddCell(new PdfPCell(new Phrase("Depósito:", normal))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			});

			subTablaC3.AddCell(new PdfPCell(new Phrase(reg.depo_nombre, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			});

			subTablaC3.AddCell(new PdfPCell(new Phrase(reg.ep_fecha.ToString("dd/MM/yyyy"), normal))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			});

			PdfPCell celdaC3 = new PdfPCell(subTablaC3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};

			tabla.AddCell(celdaC3);

			// ============================
			// ENCAPSULAR TODO EN UN RECUADRO
			// ============================
			PdfPTable tablaFinal = new PdfPTable(1);
			tablaFinal.WidthPercentage = 100;

			PdfPCell celdaContenedora = new PdfPCell(tabla)
			{
				Border = Rectangle.BOX,
				PaddingTop = 3f,
				PaddingBottom = 3f,
				PaddingLeft = 6f,
				PaddingRight = 6f
			};

			// Grosor del borde
			celdaContenedora.BorderWidth = 2f;

			// Eliminar espacio extra arriba
			tabla.SpacingBefore = 0f;

			tablaFinal.AddCell(celdaContenedora);

			return tablaFinal;
		}

		public static void CargarRepoDePlanillaDeElabYFraccion(Document pdf, List<PlanillaElabYFraccionDto> registros, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var r0 = registros.First();

			// Espacio
			pdf.Add(new Paragraph(" ", normal));
			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// ENCABEZADO — LÍNEA 1
			// ============================
			PdfPTable tblHeader1 = new PdfPTable(2);
			tblHeader1.WidthPercentage = 100;
			tblHeader1.SetWidths(new float[] { 15, 85 });

			tblHeader1.AddCell(HeaderLabel("Fecha:", normal, Element.ALIGN_RIGHT));
			tblHeader1.AddCell(HeaderValue(r0.ep_fecha.ToString("dd/MM/yyyy"), normalBold));

			pdf.Add(tblHeader1);

			// ============================
			// ENCABEZADO — LÍNEA 2
			// ============================
			PdfPTable tblHeader2 = new PdfPTable(2);
			tblHeader2.WidthPercentage = 100;
			tblHeader2.SetWidths(new float[] { 15, 85 });

			tblHeader2.AddCell(HeaderLabel("Responsable:", normal, Element.ALIGN_RIGHT));
			tblHeader2.AddCell(HeaderValue(r0.usu_apellidoynombre, normalBold));

			pdf.Add(tblHeader2);

			// ============================
			// ENCABEZADO — LÍNEA 3
			// ============================
			PdfPTable tblHeader3 = new PdfPTable(2);
			tblHeader3.WidthPercentage = 100;
			tblHeader3.SetWidths(new float[] { 15, 85 });

			tblHeader3.AddCell(HeaderLabel("Observaciones:", normal, Element.ALIGN_RIGHT));
			tblHeader3.AddCell(HeaderValue(r0.ep_motivo, normalBold));

			pdf.Add(tblHeader3);

			// Espacio
			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// LÍNEA SEPARADORA
			// ============================
			var separador = new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);
			pdf.Add(new Chunk(separador));

			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// SEPARAR PRODUCTOS POR TIPO
			// ============================
			var elaborados = registros.Where(x => x.epd_tipo == "E").ToList();
			var materiaPrima = registros.Where(x => x.epd_tipo == "S").ToList();

			// ============================
			// TABLA — PRODUCTOS ELABORADOS
			// ============================
			Paragraph tituloElab = new Paragraph("Productos Elaborados o Fraccionados", normalBold);
			tituloElab.Alignment = Element.ALIGN_CENTER;
			tituloElab.SpacingAfter = 5f;
			pdf.Add(tituloElab);

			PdfPTable tblElab = new PdfPTable(3);
			tblElab.WidthPercentage = 75;
			tblElab.HorizontalAlignment = Element.ALIGN_CENTER;
			tblElab.SetWidths(new float[] { 15, 65, 20 });
			tblElab.HeaderRows = 1;

			tblElab.AddCell(HeaderCell("Código", normalBold));
			tblElab.AddCell(HeaderCell("Descripción", normalBold));
			tblElab.AddCell(HeaderCell("Cantidad", normalBold));

			foreach (var x in elaborados)
			{
				tblElab.AddCell(Cell(x.p_id, normal, Element.ALIGN_CENTER));
				tblElab.AddCell(Cell(x.p_desc, normal, Element.ALIGN_LEFT));

				string cantidad = GridHelper.FormatearDato(x.epd_cantidad, GridHelper.FormatDato.Monto, x.PermiteDecimales);
				tblElab.AddCell(Cell(cantidad, normal, Element.ALIGN_RIGHT));
			}

			pdf.Add(tblElab);

			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// TABLA — MATERIA PRIMA
			// ============================
			Paragraph tituloMp = new Paragraph("Productos Utilizados como Materia Prima", normalBold);
			tituloMp.Alignment = Element.ALIGN_CENTER;
			tituloMp.SpacingAfter = 5f;
			pdf.Add(tituloMp);

			PdfPTable tblMp = new PdfPTable(3);
			tblMp.WidthPercentage = 75;
			tblMp.HorizontalAlignment = Element.ALIGN_CENTER;
			tblMp.SetWidths(new float[] { 15, 65, 20 });
			tblMp.HeaderRows = 1;

			tblMp.AddCell(HeaderCell("Código", normalBold));
			tblMp.AddCell(HeaderCell("Descripción", normalBold));
			tblMp.AddCell(HeaderCell("Cantidad", normalBold));

			foreach (var x in materiaPrima)
			{
				tblMp.AddCell(Cell(x.p_id, normal, Element.ALIGN_CENTER));
				tblMp.AddCell(Cell(x.p_desc, normal, Element.ALIGN_LEFT));

				string cantidad = GridHelper.FormatearDato(x.epd_cantidad, GridHelper.FormatDato.Monto, x.PermiteDecimales);
				tblMp.AddCell(Cell(cantidad, normal, Element.ALIGN_RIGHT));
			}

			pdf.Add(tblMp);
		}

		private static PdfPCell HeaderLabel(string texto, Font font, int align = Element.ALIGN_LEFT)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.Border = Rectangle.NO_BORDER;
			c.HorizontalAlignment = align;
			return c;
		}

		private static PdfPCell HeaderValue(string texto, Font font)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.Border = Rectangle.NO_BORDER;
			c.HorizontalAlignment = Element.ALIGN_LEFT;
			return c;
		}

		private static PdfPCell HeaderCell(string texto, Font font)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.BackgroundColor = new BaseColor(230, 230, 230);
			return c;
		}

		private static PdfPCell Cell(string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			return c;
		}

		#endregion

		private List<PlanillaElabYFraccionDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			try
			{
				var ret = new List<PlanillaElabYFraccionDto>();
				var pe_compte = solicitud.Parametros.GetValueOrDefault("pe_compte", "")?.ToString() ?? null;

				ret = _prdSrv.ObtenerPlanillaDeElabYFraccion(pe_compte);
				titulo = $"Planilla de Elaboración o Fracción N° {pe_compte}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<PlanillaElabYFraccionDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<PlanillaElabYFraccionDto> registros = ObtenerDatos(solicitud, out tit);

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
