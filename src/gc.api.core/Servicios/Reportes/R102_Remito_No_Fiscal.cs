using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R102_Remito_No_Fiscal : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R102_Remito_No_Fiscal(IUnitOfWork uow, IApiProductoServicio apiProdSrv,
									IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiProdSrv = apiProdSrv;
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
				string smTipo;
				string filtrosString;
				List<RemitoNoFiscalDto> registros = ObtenerDatos(solicitud, out tit, out smTipo, out filtrosString);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = string.Empty;

				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{

				}).ToList();

				#endregion
				#region Scripts PDF
				#region instanciamos el pdf
				pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);
				var reg = registros.FirstOrDefault();
				// Agregar el evento de pie de página
				writer.PageEvent = new RemitoPageEvent(solicitud.Observacion, 40f, reg.cai, Convert.ToDateTime(reg.cai_vto).ToString("dd/MM/yyyy"));

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
				// ============================
				// Si NO hay registros → generar reporte vacío
				// ============================
				if (registros.Count == 0)
				{
					pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

					writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

					//var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

					pdf.Open();

					// Espacio
					pdf.Add(new Paragraph(" ", normal));
					pdf.Add(new Paragraph(" ", normal));

					// Mensaje central
					Paragraph msg = new Paragraph("No hay registros para imprimir", tituloBig)
					{
						Alignment = Element.ALIGN_CENTER,
						SpacingBefore = 40f
					};
					pdf.Add(msg);

					pdf.Close();

					return Convert.ToBase64String(ms.ToArray());
				}

				var emisor = registros.First().emisor_nombre;
				var nro_remito = registros.First().emisor_nombre;
				PdfPTable tabla = GeneraCabeceraPDF2_ParaRemito(solicitud, chico, chicoBold, normal, normalBold, titulo, tituloBig, logo, _empresaGeco, registros.First());

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
				CargarDatosDelRemito(pdf, registros, smTipo, chicoplus, normal, normalBold, titulo, tituloBig);
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
		public static void CargarDatosDelRemito(Document pdf, List<RemitoNoFiscalDto> registros, string smTipo, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Espacio
			pdf.Add(new Paragraph(" ", normal));
			pdf.Add(new Paragraph(" ", normal));

			var r0 = registros.First();

			// ============================
			// ENCABEZADO — LÍNEA 1
			// ============================
			if (smTipo == "DV")
			{
				PdfPTable tblHeader = new PdfPTable(2);
				tblHeader.WidthPercentage = 100;
				tblHeader.SetWidths(new float[] { 10, 90 });

				tblHeader.AddCell(HeaderLabel("Proveedor:", normal));
				tblHeader.AddCell(HeaderValue(r0.cm_nombre, normalBold));
				pdf.Add(tblHeader);
			}
			else if (smTipo == "RE")
			{
				PdfPTable tblHeader = new PdfPTable(4);
				tblHeader.WidthPercentage = 100;
				tblHeader.SetWidths(new float[] { 10, 40, 10, 40 });

				tblHeader.AddCell(HeaderLabel("Cliente:", normal));
				tblHeader.AddCell(HeaderValue($"({r0.cta_id}) {r0.cm_nombre}", normalBold));

				tblHeader.AddCell(HeaderLabel("Cotización:", normal));
				tblHeader.AddCell(HeaderValue(r0.pre_id, normalBold));

				pdf.Add(tblHeader);
			}
			else
			{
				PdfPTable tblHeader = new PdfPTable(4);
				tblHeader.WidthPercentage = 100;
				tblHeader.SetWidths(new float[] { 15, 55, 10, 20 });

				tblHeader.AddCell(HeaderLabel("Razón Social:", normal));
				tblHeader.AddCell(HeaderValue(r0.emisor_nombre, normalBold));

				tblHeader.AddCell(HeaderValue(string.Empty, normal));
				tblHeader.AddCell(HeaderValue(r0.emisor_afip_desc, normalBold));

				pdf.Add(tblHeader);
			}

			// ============================
			// ENCABEZADO — LÍNEA 2
			// ============================
			if (smTipo == "RE" || smTipo == "DV")
			{
				PdfPTable tblHeader2 = new PdfPTable(4);
				tblHeader2.WidthPercentage = 100;
				tblHeader2.SetWidths(new float[] { 10, 40, 15, 35 });

				tblHeader2.AddCell(HeaderLabel("Domicilio:", normal));
				tblHeader2.AddCell(HeaderValue(r0.cm_domicilio, normalBold));

				tblHeader2.AddCell(HeaderLabel("Depósito:", normal));
				tblHeader2.AddCell(HeaderValue(r0.depo_nombre, normalBold));

				pdf.Add(tblHeader2);
			}
			else
			{
				PdfPTable tblHeader2 = new PdfPTable(4);
				tblHeader2.WidthPercentage = 100;
				tblHeader2.SetWidths(new float[] { 15, 55, 15, 15 });

				tblHeader2.AddCell(HeaderLabel("Domicilio:", normal));
				tblHeader2.AddCell(HeaderValue(r0.cm_domicilio, normalBold));

				tblHeader2.AddCell(HeaderLabel("Control Interno:", normal));
				tblHeader2.AddCell(HeaderValue(r0.sm_compte, normalBold));

				pdf.Add(tblHeader2);
			}

			// ============================
			// ENCABEZADO — LÍNEA 3
			// ============================
			if (smTipo == "RE" || smTipo == "DV")
			{
				PdfPTable tblHeader3 = new PdfPTable(6);
				tblHeader3.WidthPercentage = 100;
				tblHeader3.SetWidths(new float[] { 10, 20, 15, 25, 15, 15 });

				tblHeader3.AddCell(HeaderLabel("CUIT/CUIL:", normal));
				tblHeader3.AddCell(HeaderValue(r0.cm_cuit, normalBold));

				tblHeader3.AddCell(HeaderLabel("Generado por:", normal));
				tblHeader3.AddCell(HeaderValue(r0.usu_apellidoynombre, normalBold));

				tblHeader3.AddCell(HeaderLabel("Control Interno:", normal));
				tblHeader3.AddCell(HeaderValue(r0.sm_compte, normalBold));

				pdf.Add(tblHeader3);
			}
			
			// Espacio
			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// LÍNEA SEPARADORA
			// ============================
			PdfPTable lineaSep = new PdfPTable(1);
			lineaSep.WidthPercentage = 100;

			PdfPCell celdaLinea = new PdfPCell(new Phrase(" "))
			{
				Border = Rectangle.NO_BORDER,
				BorderWidthTop = 1f,   // grosor de la línea
				Padding = 0f,
				FixedHeight = 5f       // altura mínima para que la línea se vea
			};

			lineaSep.AddCell(celdaLinea);
			pdf.Add(lineaSep);

			// Espacio opcional
			pdf.Add(new Paragraph(" ", chico));

			// ============================
			// TABLA DE PRODUCTOS
			// ============================
			PdfPTable tbl;
			if (smTipo != "TR")
			{
				tbl = new PdfPTable(5);
				tbl.SetWidths(new float[] { 10, 10, 40, 10, 10 });
				tbl.WidthPercentage = 100;
				tbl.HeaderRows = 1; // repetir cabecera en todas las hojas

				// Encabezados
				tbl.AddCell(HeaderCellBorde("Item", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Código", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Descripción", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Entregado", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Costo", normalBold, Element.ALIGN_CENTER));
				foreach (var x in registros)
				{
					tbl.AddCell(CellBorde(x.item.ToString(), normal, Element.ALIGN_CENTER));
					tbl.AddCell(CellBorde(x.p_id, normal, Element.ALIGN_CENTER));
					tbl.AddCell(CellBorde(x.p_id_desc, normal, Element.ALIGN_LEFT));

					string cantidad = GridHelper.FormatearDato(x.cantidad, GridHelper.FormatDato.Monto, x.PermiteDecimales);
					tbl.AddCell(CellBorde(cantidad, normal, Element.ALIGN_RIGHT));

					string costo = x.costo.ToString("N2");
					tbl.AddCell(CellBorde(costo, normal, Element.ALIGN_RIGHT));
				}
			}
			else
			{
				tbl = new PdfPTable(6);
				tbl.SetWidths(new float[] { 10, 10, 40, 10, 15, 15 });
				tbl.WidthPercentage = 100;
				tbl.HeaderRows = 1; // repetir cabecera en todas las hojas

				// Encabezados
				tbl.AddCell(HeaderCellBorde("Item", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Código", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Productos", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Ref. Prov.", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Unid. Pres.", normalBold, Element.ALIGN_CENTER));
				tbl.AddCell(HeaderCellBorde("Cantidad", normalBold, Element.ALIGN_CENTER));
				foreach (var x in registros)
				{
					tbl.AddCell(CellBorde(x.item.ToString(), normal, Element.ALIGN_CENTER));
					tbl.AddCell(CellBorde(x.p_id, normal, Element.ALIGN_CENTER));
					tbl.AddCell(CellBorde(x.p_id_desc, normal, Element.ALIGN_LEFT));
					tbl.AddCell(CellBorde(x.p_id_prov, normal, Element.ALIGN_LEFT));
					tbl.AddCell(CellBorde(x.unidad_pres.ToString(), normal, Element.ALIGN_RIGHT));
					string cantidad = GridHelper.FormatearDato(x.cantidad, GridHelper.FormatDato.Monto, x.PermiteDecimales);
					tbl.AddCell(CellBorde(cantidad, normal, Element.ALIGN_RIGHT));
				}
			}

			pdf.Add(tbl);

			// Espacio opcional
			pdf.Add(new Paragraph(" ", chico));

			if (smTipo != "TR")
			{
				// ============================
				// TOTALIZADOR DE COSTO
				// ============================
				decimal totalCosto = registros.Sum(x => x.costo);

				PdfPTable tblTotal = new PdfPTable(3);
				tblTotal.WidthPercentage = 100;                 // ancho del bloque
				tblTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
				tblTotal.SetWidths(new float[] { 80, 10, 10 });    // etiqueta / valor

				PdfPCell lblObs = new PdfPCell(new Phrase("Obs: NO APTA PARA LA VENTA", normal))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingTop = 2f,
					PaddingBottom = 2f
				};
				tblTotal.AddCell(lblObs);

				// Etiqueta
				PdfPCell lbl = new PdfPCell(new Phrase("Total Costo:", normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingTop = 2f,
					PaddingBottom = 2f
				};
				tblTotal.AddCell(lbl);

				// Valor
				PdfPCell val = new PdfPCell(new Phrase(totalCosto.ToString("N2"), normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					PaddingTop = 2f,
					PaddingBottom = 2f
				};
				tblTotal.AddCell(val);

				// Agregar al PDF
				pdf.Add(tblTotal);
			}
			else
			{
				// Espacio
				pdf.Add(new Paragraph(" ", normal));
				pdf.Add(new Paragraph(" ", normal));
				pdf.Add(new Paragraph(" ", normal));

				// ============================
				// LÍNEA HORIZONTAL (alineada a la derecha)
				// ============================

				PdfPTable lineaFirma = new PdfPTable(1);
				lineaFirma.WidthPercentage = 40; // alineado a la derecha
				lineaFirma.HorizontalAlignment = Element.ALIGN_RIGHT;

				PdfPCell celdaLineaFirma = new PdfPCell(new Phrase(" "))
				{
					Border = Rectangle.NO_BORDER,
					BorderWidthTop = 1.2f,   // grosor de la línea
					Padding = 0f,
					FixedHeight = 12f        // altura mínima para que la línea se vea
				};

				lineaFirma.AddCell(celdaLineaFirma);
				pdf.Add(lineaFirma);

				// ============================
				// LEYENDA DE FIRMA
				// ============================

				PdfPTable leyendaFirma = new PdfPTable(1);
				leyendaFirma.WidthPercentage = 40;
				leyendaFirma.HorizontalAlignment = Element.ALIGN_RIGHT;

				PdfPCell celdaLeyenda = new PdfPCell(new Phrase("Aclaración y Firma Responsable", normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingTop = 4f
				};

				leyendaFirma.AddCell(celdaLeyenda);
				pdf.Add(leyendaFirma);
			}

		}

		// CELDAS DEL ENCABEZADO
		// ============================

		private static PdfPCell HeaderLabel(string texto, Font font)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = Element.ALIGN_RIGHT;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Border = Rectangle.NO_BORDER;
			c.Padding = 2f;
			return c;
		}

		private static PdfPCell HeaderValue(string texto, Font font)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = Element.ALIGN_LEFT;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Border = Rectangle.NO_BORDER;
			c.Padding = 2f;
			return c;
		}

		// ============================
		// CELDAS CON BORDE (TABLA)
		// ============================

		public static PdfPCell CellBorde(string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto ?? "", font));
			c.HorizontalAlignment = align;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Border = Rectangle.BOX;
			c.BorderWidth = 0.5f;
			c.Padding = 3f;
			return c;
		}

		public static PdfPCell HeaderCellBorde(string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto ?? "", font));
			c.HorizontalAlignment = align;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.BackgroundColor = new BaseColor(230, 230, 230);
			c.Border = Rectangle.BOX;
			c.BorderWidth = 0.5f;
			c.Padding = 4f;
			return c;
		}
		#endregion

		private List<RemitoNoFiscalDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string smTipo, out string filtrosString)
		{
			try
			{
				var ret = new List<RemitoNoFiscalDto>();
				var sm_tipo = solicitud.Parametros.GetValueOrDefault("sm_tipo", "")?.ToString() ?? null;
				var sm_compte = solicitud.Parametros.GetValueOrDefault("id", "")?.ToString() ?? null;
				var costo = true;
				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;

				if (sm_compte == null)
				{
					titulo = "";
					smTipo = "";
					filtrosString = "";
					return [];
				}
				ret = _apiProdSrv.ObtenerRemitoNoFiscal(new RemitoNoFiscalRequest()
				{
					costo = costo,
					sm_compte = sm_compte,
					sm_tipo = sm_tipo
				});
				titulo = $"N° Remito {sm_compte}";
				smTipo = sm_tipo;
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				smTipo = "";
				filtrosString = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string smTipo;
			string filtrosString;
			List<RemitoNoFiscalDto> registros = ObtenerDatos(solicitud, out tit, out smTipo, out filtrosString);

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
			string smTipo;
			string filtrosString;
			List<RemitoNoFiscalDto> registros = ObtenerDatos(solicitud, out tit, out smTipo, out filtrosString);

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

		public class RemitoPageEvent : PdfPageEventHelper
		{
			private readonly string _footerText;
			private readonly float _espacioEncabezado;
			private readonly string _cai;
			private readonly string _caiVto;

			private PdfTemplate _totalPages;
			private BaseFont _baseFont;

			public float MargenInferior { get; set; } = 15;

			public RemitoPageEvent(string footerText, float espacioEncabezado, string cai, string caiVto)
			{
				_footerText = footerText;
				_espacioEncabezado = espacioEncabezado;
				_cai = cai;
				_caiVto = caiVto;
			}

			public override void OnOpenDocument(PdfWriter writer, Document document)
			{
				_totalPages = writer.DirectContent.CreateTemplate(50, 20);
				_baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			}

			public override void OnStartPage(PdfWriter writer, Document document)
			{
				// En la primera página NO agregamos espacio
				if (writer.PageNumber == 1)
					return;

				// Reservar espacio real en el flujo del documento
				PdfPTable espacio = new PdfPTable(1);
				espacio.WidthPercentage = 100;

				PdfPCell celda = new PdfPCell(new Phrase(" "))
				{
					Border = Rectangle.NO_BORDER,
					FixedHeight = _espacioEncabezado
				};

				espacio.AddCell(celda);

				document.Add(espacio);
			}

			public override void OnEndPage(PdfWriter writer, Document document)
			{
				PdfContentByte cb = writer.DirectContent;
				float pageWidth = document.PageSize.Width;

				float footerY = document.BottomMargin - MargenInferior;

				// ============================
				// LÍNEA HORIZONTAL DEL FOOTER
				// ============================
				cb.SetLineWidth(0.5f);
				cb.MoveTo(document.LeftMargin, footerY + 15);
				cb.LineTo(pageWidth - document.RightMargin, footerY + 15);
				cb.Stroke();

				Font footerFont = new Font(_baseFont, 8, Font.NORMAL);

				// ============================
				// TABLA DEL PIE DE PÁGINA
				// ============================
				PdfPTable footerTable = new PdfPTable(3);
				footerTable.TotalWidth = pageWidth - document.LeftMargin - document.RightMargin;
				footerTable.SetWidths(new float[] { 35f, 20f, 45f });
				footerTable.DefaultCell.Border = Rectangle.NO_BORDER;

				string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

				footerTable.AddCell(new PdfPCell(new Phrase($"Fecha de Impresión: {currentDate}", footerFont))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingTop = 3
				});

				footerTable.AddCell(new PdfPCell(new Phrase(_footerText, footerFont))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingTop = 3
				});

				Phrase pagePhrase = new Phrase($"Página {writer.PageNumber} de ", footerFont);
				pagePhrase.Add(new Chunk(Image.GetInstance(_totalPages), 0, 0, true));

				footerTable.AddCell(new PdfPCell(pagePhrase)
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					PaddingTop = 3
				});

				footerTable.WriteSelectedRows(0, -1, document.LeftMargin, footerY + 3, cb);

				// ============================
				// CAI centrado sobre el pie de página
				// ============================
				string textoCai = $"CAI: {_cai} - Vto: {_caiVto}";
				Font caiFont = new Font(_baseFont, 9, Font.BOLD);

				PdfPTable tblCai = new PdfPTable(1);
				tblCai.TotalWidth = pageWidth - document.LeftMargin - document.RightMargin;

				PdfPCell celdaCai = new PdfPCell(new Phrase(textoCai, caiFont))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingTop = 4f,
					PaddingBottom = 2f
				};

				tblCai.AddCell(celdaCai);

				float posY = footerY + 40; // Ajustable según estética

				tblCai.WriteSelectedRows(0, -1, document.LeftMargin, posY, cb);
			}

			public override void OnCloseDocument(PdfWriter writer, Document document)
			{
				_totalPages.BeginText();
				_totalPages.SetFontAndSize(_baseFont, 8);
				_totalPages.SetTextMatrix(0, 0);
				_totalPages.ShowText((writer.PageNumber - 1).ToString());
				_totalPages.EndText();
			}
		}

	}
}
