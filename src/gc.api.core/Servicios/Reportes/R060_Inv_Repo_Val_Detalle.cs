using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R060_Inv_Repo_Val_Detalle : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IInventarioServicio _inventarioServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R060_Inv_Repo_Val_Detalle(IUnitOfWork uow, IInventarioServicio inventarioServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_inventarioServicio = inventarioServicio;
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
				List<InvRepoValorDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoValorDetalle(pdf, registros, chico, normalBold, titulo);
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

		public static bool GetBoolParam(IDictionary<string, string> parametros, string clave, bool valorPorDefecto = false)
		{
			if (parametros == null || !parametros.TryGetValue(clave, out var valor) || string.IsNullOrWhiteSpace(valor))
				return valorPorDefecto;

			return bool.TryParse(valor, out var resultado) ? resultado : valorPorDefecto;
		}

		private List<InvRepoValorDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			var inv_nro = solicitud.Parametros.GetValueOrDefault("inv_nro", "")?.ToString() ?? null;

			titulo = $"Inventario Valorizado N° {inv_nro}";
			
			var lista =  _inventarioServicio.GetReporteValorizadoDetalle(new ReporteInventarioRequest
			{
				inv_nro = inv_nro
			});
			subtitulo = $"Estado: {lista.First().inve_desc}";
			return lista;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<InvRepoValorDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<InvRepoValorDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoValorDetalle(Document pdf, List<InvRepoValorDetalleDto> lista, Font fChico, Font fNormal, Font fTitulo)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fNormal));
				return;
			}

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);

			var grupos = lista
				.GroupBy(x => new { x.sec_id, x.sec_desc, x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.sec_id)
				.ThenBy(g => g.Key.rub_id);

			bool primera = true;

			foreach (var grupo in grupos)
			{
				if (!primera)
					pdf.NewPage();
				primera = false;

				var datos = grupo.ToList();

				int prodDelRubro = datos.Count;
				int prodConAjuste = datos.Count(x => x.ps_ajuste == 'S');
				decimal prodConConteo = prodDelRubro == 0 ? 0 : (prodConAjuste * 100m / prodDelRubro);

				decimal valorProdSinConteo = datos.Where(y => y.conteo1 == 0)
												  .Sum(x => x.ps_stk * x.p_costo);

				decimal valorProdConConteo = datos.Where(y => y.conteo1 > 0)
												  .Sum(x => (x.ps_conteo - x.ps_stk) * x.p_costo);

				decimal stkTotal = datos.Sum(x => x.ps_stk);
				decimal conteoTotal = datos.Sum(x => x.ps_conteo);
				decimal difTotal = stkTotal - conteoTotal;

				decimal stkTotalVal = datos.Sum(x => x.ps_stk * x.p_costo);
				decimal conteoTotalVal = datos.Sum(x => x.ps_conteo * x.p_costo);
				decimal difTotalVal = stkTotalVal - conteoTotalVal;

				// ============================================================
				// TABLA PRINCIPAL DEL GRUPO (título + cabecera + datos)
				// ============================================================
				PdfPTable tabla = new PdfPTable(9);
				tabla.WidthPercentage = 100;

				// Anchos equivalentes al <colgroup>
				tabla.SetWidths(new float[] { 10, 45, 10, 7, 7, 7, 7, 7, 7 });

				// ============================================================
				// FILA AGRUPADORA (colspan=9)
				// ============================================================
				PdfPCell celdaAgr = new PdfPCell();
				celdaAgr.Colspan = 9;
				celdaAgr.BackgroundColor = new BaseColor(230, 230, 230);
				celdaAgr.Padding = 6f;

				// Construimos el contenido del agrupador
				PdfPTable tAgr = new PdfPTable(2);
				tAgr.WidthPercentage = 100;
				tAgr.SetWidths(new float[] { 60, 40 });

				// ------------------ Columna izquierda (título + datos)
				PdfPTable tIzq = new PdfPTable(1);
				tIzq.WidthPercentage = 100;

				PdfPCell t1 = new PdfPCell(new Phrase(
					$"Sector: {grupo.Key.sec_desc} - Rubro: {grupo.Key.rub_desc}", fTitulo));
				t1.Border = Rectangle.NO_BORDER;
				tIzq.AddCell(t1);

				PdfPCell t2 = new PdfPCell(new Phrase(
					$"Prod. del Rubro: {prodDelRubro}\n" +
					$"Prod. con Conteo: {prodConConteo:N2}%\n" +
					$"Valoriza Prod. sin Conteo: {valorProdSinConteo:N2}\n" +
					$"Valoriza Dif. Prod. con Conteo: {valorProdConConteo:N2}", fNormal));
				t2.Border = Rectangle.NO_BORDER;
				tIzq.AddCell(t2);

				// ------------------ Columna derecha (mini‑tablas)
				PdfPTable tDer = new PdfPTable(2);
				tDer.WidthPercentage = 100;
				tDer.SetWidths(new float[] { 50, 50 });

				// Mini tabla Cantidades
				PdfPTable tCant = new PdfPTable(3);
				tCant.WidthPercentage = 100;
				tCant.AddCell(CeldaMiniHeader("Cantidades", 3, fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Stk", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Conteo", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Dif", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini(stkTotal.ToString("N2"), fChico));
				tCant.AddCell(CeldaMini(conteoTotal.ToString("N2"), fChico));
				tCant.AddCell(CeldaMini(difTotal.ToString("N2"), fChico));

				// Mini tabla Valorización
				PdfPTable tVal = new PdfPTable(3);
				tVal.WidthPercentage = 100;
				tVal.AddCell(CeldaMiniHeader("Valorización", 3, fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Stk", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Conteo", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Dif", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini(stkTotalVal.ToString("N2"), fChico));
				tVal.AddCell(CeldaMini(conteoTotalVal.ToString("N2"), fChico));
				tVal.AddCell(CeldaMini(difTotalVal.ToString("N2"), fChico));

				tDer.AddCell(Wrap(tCant));
				tDer.AddCell(Wrap(tVal));

				// Armamos la fila agrupadora
				tAgr.AddCell(Wrap(tIzq));
				tAgr.AddCell(Wrap(tDer));

				celdaAgr.AddElement(tAgr);
				tabla.AddCell(celdaAgr);

				// ============================================================
				// CABECERA DEL DETALLE (2 filas)
				// ============================================================
				BaseColor dorado = new BaseColor(186, 134, 11);

				tabla.AddCell(CeldaHeader("Código", fNormal, amarilloPastel, 2));
				tabla.AddCell(CeldaHeader("Descripción", fNormal, amarilloPastel, 2));
				tabla.AddCell(CeldaHeader("Ajuste", fNormal, amarilloPastel, 2));

				tabla.AddCell(CeldaHeader("Cantidades", fNormal, amarilloPastel, 1, 3));
				tabla.AddCell(CeldaHeader("Valorización", fNormal, amarilloPastel, 1, 3));

				tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Conteo", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Dif", fNormal, amarilloPastel));

				tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Conteo", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Dif", fNormal, amarilloPastel));

				// ============================================================
				// FILAS DE PRODUCTOS
				// ============================================================
				bool alt = true;

				foreach (var item in datos)
				{
					var difCant = item.ps_stk - item.ps_conteo;
					var stkVal = item.ps_stk * item.p_costo;
					var conVal = item.ps_conteo * item.p_costo;
					var difVal = stkVal - conVal;

					BaseColor fondo = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
					alt = !alt;

					tabla.AddCell(CeldaDato(item.p_id, fChico, fondo));
					tabla.AddCell(CeldaDato(item.p_des, fChico, fondo));
					tabla.AddCell(CeldaDato(item.ps_ajuste == 'S' ? "✔" : "✘", fChico, fondo, Element.ALIGN_CENTER));

					tabla.AddCell(CeldaDato(item.ps_stk.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(item.ps_conteo.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(difCant.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

					tabla.AddCell(CeldaDato(stkVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(conVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(difVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
				}

				pdf.Add(tabla);
			}
		}

		private static PdfPCell CeldaDato(string texto, Font f, BaseColor fondo, int align = Element.ALIGN_LEFT)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = fondo;
			c.HorizontalAlignment = align;
			return c;
		}

		private static PdfPCell CeldaHeader(string texto, Font f, BaseColor color, int rowspan = 1, int colspan = 1)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = color;
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Rowspan = rowspan;
			c.Colspan = colspan;
			return c;
		}

		private static PdfPCell CeldaMiniHeader(string texto, int colspan, Font f, BaseColor color)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.Colspan = colspan;
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.BackgroundColor = color;
			return c;
		}

		private static PdfPCell CeldaMini(string texto, Font f, BaseColor color)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.BackgroundColor = color;
			return c;
		}

		public static PdfPCell CeldaMini(string texto, Font fuente)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, fuente));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			return c;
		}

		private static PdfPCell Wrap(PdfPTable t)
		{
			PdfPCell c = new PdfPCell();
			c.AddElement(t);
			c.Border = Rectangle.NO_BORDER;
			return c;
		}
		#endregion
	}
}
