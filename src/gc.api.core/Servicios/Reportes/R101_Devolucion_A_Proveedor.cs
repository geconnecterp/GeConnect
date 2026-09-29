using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
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
	public class R101_Devolucion_A_Proveedor : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R101_Devolucion_A_Proveedor(IUnitOfWork uow, IApiProductoServicio apiProdSrv,
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
				string subtit;
				string filtrosString;
				List<DevolucionRevertidoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subtit;

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
				CargarDevolucionAProveedor(pdf, registros, chicoplus, normal, normalBold, titulo, tituloBig);
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
				throw new NegocioException("Se produjo un error al intentar generar el Reporte Devolucion a Proveedores. Para mayores datos ver el log.");
			}
		}

		#region Funciones de generacion de secciones de reportes
		public static void CargarDevolucionAProveedor(Document pdf, List<DevolucionRevertidoDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var r0 = registros.First();

			// ============================
			// ENCABEZADO — LÍNEA 1
			// ============================

			PdfPTable tblHeader = new PdfPTable(4);
			tblHeader.WidthPercentage = 100;
			tblHeader.SetWidths(new float[] { 10, 40, 20, 30 });

			tblHeader.AddCell(HeaderLabel("Proveedor:", normalBold));
			tblHeader.AddCell(HeaderValue(r0.cta_denominacion, normal));

			tblHeader.AddCell(HeaderLabel("Remito Asociado:", normalBold));
			tblHeader.AddCell(HeaderValue(r0.dv_compte, normal)); // Reemplazar por el dato correcto

			pdf.Add(tblHeader);

			// ============================
			// ENCABEZADO — LÍNEA 2
			// ============================

			PdfPTable tblHeader2 = new PdfPTable(6);
			tblHeader2.WidthPercentage = 100;
			tblHeader2.SetWidths(new float[] { 10, 20, 10, 20, 10, 20 });

			tblHeader2.AddCell(HeaderLabel("Fecha:", normalBold));
			tblHeader2.AddCell(HeaderValue(r0.dv_fecha.ToString("dd/MM/yyyy"), normal));

			tblHeader2.AddCell(HeaderLabel("Generaro Por:", normalBold));
			tblHeader2.AddCell(HeaderValue(r0.usu_apellidoynombre, normal));

			tblHeader2.AddCell(HeaderLabel("NC a Cta. Asociada:", normalBold));
			tblHeader2.AddCell(HeaderValue("", normal));

			pdf.Add(tblHeader2);

			// Espacio
			pdf.Add(new Paragraph(" ", normal));

			// ============================
			// TABLA DE PRODUCTOS
			// ============================

			PdfPTable tbl = new PdfPTable(5);
			tbl.WidthPercentage = 100;
			tbl.SetWidths(new float[] { 10, 40, 10, 10, 10 });
			tbl.HeaderRows = 1; // repetir cabecera en todas las hojas

			// Encabezados
			tbl.AddCell(HeaderCellBorde("Item", normalBold, Element.ALIGN_CENTER));
			tbl.AddCell(HeaderCellBorde("Producto", normalBold, Element.ALIGN_CENTER));
			tbl.AddCell(HeaderCellBorde("Cantidad", normalBold, Element.ALIGN_CENTER));
			tbl.AddCell(HeaderCellBorde("Precio", normalBold, Element.ALIGN_CENTER));
			tbl.AddCell(HeaderCellBorde("SubTotal", normalBold, Element.ALIGN_CENTER));
			var item = 1;
			foreach (var x in registros)
			{
				tbl.AddCell(CellBorde((item++).ToString(), normal, Element.ALIGN_CENTER));
				tbl.AddCell(CellBorde($"{x.p_id} {x.p_desc}", normal, Element.ALIGN_LEFT));

				string cantidad = GridHelper.FormatearDato(x.dvd_cantidad, GridHelper.FormatDato.Monto, x.PermiteDecimales);
				tbl.AddCell(CellBorde(cantidad, normal, Element.ALIGN_RIGHT));

				string costo = x.dvd_pcosto.ToString("N2");
				tbl.AddCell(CellBorde(costo, normal, Element.ALIGN_RIGHT));

				decimal subTotal = x.dvd_cantidad * x.dvd_pcosto;
				string subTotalFmt = subTotal.ToString("N2");
				tbl.AddCell(CellBorde(subTotalFmt, normal, Element.ALIGN_RIGHT));
			}

			pdf.Add(tbl);

			pdf.Add(new Paragraph(" ", normal));   // espacio separador
			pdf.Add(new Paragraph(" ", normal));   // si querés un poco más

			// ============================
			// CALCULOS
			// ============================

			decimal gravado = registros.Sum(x => x.dvd_cantidad * x.dvd_pcosto);
			decimal iva = (gravado * 1.21m) - gravado;
			decimal total = gravado + iva;

			Paragraph obs = new Paragraph("Obs: NO APTA PARA LA VENTA", normalBold);
			obs.Alignment = Element.ALIGN_LEFT;
			obs.SpacingBefore = 10f;
			pdf.Add(obs);

			// ============================
			// TABLA DE TOTALES
			// ============================

			PdfPTable tblTotales = new PdfPTable(2);
			tblTotales.WidthPercentage = 40;          // ancho del bloque
			tblTotales.HorizontalAlignment = Element.ALIGN_RIGHT;
			tblTotales.SetWidths(new float[] { 60, 40 });

			// Helper para celdas sin borde
			PdfPCell Label(string t)
			{
				var c = new PdfPCell(new Phrase(t, normal));
				c.Border = Rectangle.NO_BORDER;
				c.HorizontalAlignment = Element.ALIGN_LEFT;
				c.Padding = 1f;
				return c;
			}

			PdfPCell Value(string t)
			{
				var c = new PdfPCell(new Phrase(t, normalBold));
				c.Border = Rectangle.NO_BORDER;
				c.HorizontalAlignment = Element.ALIGN_RIGHT;
				c.Padding = 1f;
				return c;
			}

			PdfPCell Linea()
			{
				var c = new PdfPCell(new Phrase(" ", normal));
				c.BorderWidthTop = 0.5f;
				c.BorderWidthBottom = 0;
				c.BorderWidthLeft = 0;
				c.BorderWidthRight = 0;
				c.Colspan = 2;
				c.Padding = 0f;   // 🔥 sin espacio extra
				return c;
			}

			// No Gravado
			tblTotales.AddCell(Label("No Gravado:"));
			tblTotales.AddCell(Value("0.00"));
			tblTotales.AddCell(Linea());

			// Gravado
			tblTotales.AddCell(Label("Gravado:"));
			tblTotales.AddCell(Value(gravado.ToString("N2")));
			tblTotales.AddCell(Linea());

			// IVA
			tblTotales.AddCell(Label("IVA:"));
			tblTotales.AddCell(Value(iva.ToString("N2")));
			tblTotales.AddCell(Linea());

			// Impuestos Internos
			tblTotales.AddCell(Label("Impu. Internos:"));
			tblTotales.AddCell(Value("0.00"));
			tblTotales.AddCell(Linea());

			// TOTAL (en negrita)
			PdfPCell lblTotal = new PdfPCell(new Phrase("TOTAL:", normalBold));
			lblTotal.Border = Rectangle.NO_BORDER;
			lblTotal.HorizontalAlignment = Element.ALIGN_LEFT;
			lblTotal.Padding = 1f;

			PdfPCell valTotal = new PdfPCell(new Phrase(total.ToString("N2"), normalBold));
			valTotal.Border = Rectangle.NO_BORDER;
			valTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
			valTotal.Padding = 1f;

			tblTotales.AddCell(lblTotal);
			tblTotales.AddCell(valTotal);

			// Agregar al PDF
			pdf.Add(tblTotales);


		}

		// ============================
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

		private List<DevolucionRevertidoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string filtrosString)
		{
			try
			{
				var ret = new List<DevolucionRevertidoDto>();
				var vd_compte = solicitud.Parametros.GetValueOrDefault("id", "")?.ToString() ?? null;
				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;

				if (vd_compte == null)
				{
					titulo = "";
					subtitulo = "";
					filtrosString = "";
					return [];
				}
				ret = _apiProdSrv.ObtenerDPREVERTIDO(vd_compte);
				titulo = $"Devoluciones a Proveedor N° {vd_compte}";
				subtitulo = $"{filtrosString}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtitulo = "";
				filtrosString = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			string filtrosString;
			List<DevolucionRevertidoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
			string filtrosString;
			List<DevolucionRevertidoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
