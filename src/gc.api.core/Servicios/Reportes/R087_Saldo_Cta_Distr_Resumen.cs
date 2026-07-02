using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
				List<SaldoResumenDto> registros = ObtenerDatos(solicitud, out tit);

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
				CargarRepoSaldoResumenCtaDistribuidora(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarRepoSaldoResumenCtaDistribuidora(Document pdf, List<SaldoResumenDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Agrupación por vendedor
			var gruposVendedor = registros
				.GroupBy(x => new { x.ve_id, x.ve_nombre })
				.OrderBy(g => g.Key.ve_id);

			foreach (var grupoV in gruposVendedor)
			{
				var lista = grupoV.ToList();

				// ================================
				// ENCABEZADO DEL VENDEDOR
				// ================================
				PdfPTable tblVendedor = new PdfPTable(1);
				tblVendedor.WidthPercentage = 100;

				Chunk lblVend = new Chunk("Vendedor:", normal);
				lblVend.SetUnderline(0.5f, -2f);

				Chunk lblId = new Chunk($" ({grupoV.Key.ve_id}) ", normalBold);
				Chunk lblNom = new Chunk($"{grupoV.Key.ve_nombre}", normalBold);

				Phrase phrVend = new Phrase();
				phrVend.Add(lblVend);
				phrVend.Add(lblId);
				phrVend.Add(lblNom);

				PdfPCell celVend = new PdfPCell(phrVend);
				celVend.Border = Rectangle.NO_BORDER;
				celVend.PaddingBottom = 6;
				celVend.BackgroundColor = new BaseColor(220, 230, 255); // azul suave

				tblVendedor.AddCell(celVend);
				pdf.Add(tblVendedor);

				// ================================
				// TABLA DE RESUMEN
				// ================================
				PdfPTable tbl = new PdfPTable(6);
				tbl.WidthPercentage = 100;
				tbl.SetWidths(new float[] { 35, 13, 13, 13, 13, 13 });

				// Encabezados dinámicos
				string h13 = lista.First().hoy_m13;
				string h7 = lista.First().hoy_m7;
				string h6 = lista.First().hoy_m6;
				string h1 = lista.First().hoy_1;
				string hoy = lista.First().hoy;

				AddHeader(tbl, "Cuenta", normalBold, Element.ALIGN_LEFT);

				AddHeader(tbl, $"Ant. {h13}", normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddHeader(tbl, $"Entre {h13} al {h7}", normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddHeader(tbl, $"Entre {h6} al {h1}", normalBold, Element.ALIGN_RIGHT, BaseColor.Red);

				AddHeader(tbl, $"Tot. vencido al {hoy}", normalBold, Element.ALIGN_RIGHT, BaseColor.Red, bold: true);

				AddHeader(tbl, "A vencer", normalBold, Element.ALIGN_RIGHT, new BaseColor(0, 0, 200), bold: true);

				// Filas
				decimal totalAnt3 = 0;
				decimal totalAnt2 = 0;
				decimal totalAnt1 = 0;
				decimal totalVencido = 0;
				decimal totalAVencer = 0;

				foreach (var r in lista.OrderBy(x => x.cta_id))
				{
					decimal vencido = r.saldo_semana_ant1 + r.saldo_semana_ant2 + r.saldo_semana_ant3;

					totalAnt3 += r.saldo_semana_ant3;
					totalAnt2 += r.saldo_semana_ant2;
					totalAnt1 += r.saldo_semana_ant1;
					totalVencido += vencido;
					totalAVencer += r.saldo_avecer;

					// Cuenta
					AddCell(tbl, $"({r.cta_id}) {r.cta_denominacion}", normal, Element.ALIGN_LEFT);

					// Ant. 13
					AddCell(tbl, r.saldo_semana_ant3.ToString("N2"), normal, Element.ALIGN_RIGHT, BaseColor.Red);

					// Entre 13 al 7
					AddCell(tbl, r.saldo_semana_ant2.ToString("N2"), normal, Element.ALIGN_RIGHT, BaseColor.Red);

					// Entre 6 al 1
					AddCell(tbl, r.saldo_semana_ant1.ToString("N2"), normal, Element.ALIGN_RIGHT, BaseColor.Red);

					// Tot. vencido
					AddCell(tbl, vencido.ToString("N2"), normalBold, Element.ALIGN_RIGHT, BaseColor.Red);

					// A vencer
					AddCell(tbl, r.saldo_avecer.ToString("N2"), normalBold, Element.ALIGN_RIGHT, new BaseColor(0, 0, 200));
				}

				pdf.Add(tbl);

				// ================================
				// FILA TOTALIZADORA
				// ================================
				PdfPTable tblTot = new PdfPTable(6);
				tblTot.WidthPercentage = 100;
				tblTot.SetWidths(new float[] { 35, 13, 13, 13, 13, 13 });

				AddCell(tblTot, "Total:", normalBold, Element.ALIGN_LEFT);

				AddCell(tblTot, totalAnt3.ToString("N2"), normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddCell(tblTot, totalAnt2.ToString("N2"), normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddCell(tblTot, totalAnt1.ToString("N2"), normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddCell(tblTot, totalVencido.ToString("N2"), normalBold, Element.ALIGN_RIGHT, BaseColor.Red);
				AddCell(tblTot, totalAVencer.ToString("N2"), normalBold, Element.ALIGN_RIGHT, new BaseColor(0, 0, 200));

				pdf.Add(tblTot);

				// Separador
				PdfPTable tblSep = new PdfPTable(1);
				tblSep.WidthPercentage = 100;

				PdfPCell celSep = new PdfPCell(new Phrase(""));
				celSep.BorderWidthBottom = 1;
				celSep.BorderColorBottom = BaseColor.Gray;
				celSep.Border = Rectangle.BOTTOM_BORDER;
				celSep.PaddingBottom = 6;

				tblSep.AddCell(celSep);
				pdf.Add(tblSep);
			}
		}

		// =====================================================
		// HELPERS
		// =====================================================
		private static void AddHeader(PdfPTable tbl, string texto, Font font, int align, BaseColor color = null, bool bold = false)
		{
			Font f = bold ? new Font(font.BaseFont, font.Size, Font.BOLD, color ?? font.Color)
						  : new Font(font.BaseFont, font.Size, font.Style, color ?? font.Color);

			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.HorizontalAlignment = align;
			c.Border = Rectangle.NO_BORDER;
			c.PaddingBottom = 4;
			tbl.AddCell(c);
		}

		private static void AddCell(PdfPTable tbl, string texto, Font font, int align, BaseColor color = null)
		{
			Font f = new Font(font.BaseFont, font.Size, font.Style, color ?? font.Color);

			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.HorizontalAlignment = align;
			c.Border = Rectangle.NO_BORDER;
			tbl.AddCell(c);
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


		private List<SaldoResumenDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			try
			{
				var ret = new List<SaldoResumenDto>();
				var vend_list_temp = solicitud.Parametros.GetValueOrDefault("Vendedores", "")?.ToString() ?? null;
				List<string> lista = vend_list_temp.Split(',').ToList();
				ret = _consSrv.BuscarSaldoResumenCtaDistribuidora(new BuscarSaldoDetalleRequest()
				{
					ve_list = lista,
				});
				titulo = $"Saldo de Clientes x Vendedor al {ret.First().hoy}";
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
			List<SaldoResumenDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<SaldoResumenDto> registros = ObtenerDatos(solicitud, out tit);

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
