using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R094_Reporte_Var_Vtas_Y_Comp_Ult_Doce_M : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R094_Reporte_Var_Vtas_Y_Comp_Ult_Doce_M(IUnitOfWork uow, IConsultaServicio consSrv,
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
				string tipoReporte;
				string filtrosString;
				List<ReporteEvoVtasPerAnterioresDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
				CargarComisionDeRepartidoresResumen(pdf, registros, filtrosString, tipoReporte, chico, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarComisionDeRepartidoresResumen(Document pdf, List<ReporteEvoVtasPerAnterioresDto> registros, string filtrosString, string tipoReporte, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// ============================
			// LEYENDA (filtros)
			// ============================
			//pdf.Add(new Paragraph(filtrosString, normal));
			//pdf.Add(Chunk.Newline);

			// ============================
			// SELECCIÓN DE TABLA SEGÚN AGRUPADOR
			// ============================
			switch (tipoReporte.ToUpper())
			{
				case "SIN AGRUPAR":
					TablaSinAgrupar(pdf, registros, chico, normal, normalBold);
					break;

				case "POR RUBROS":
					TablaPorRubro(pdf, registros, chico, normal, normalBold, chicoBold);
					break;

				case "POR SECTOR":
					TablaPorSector(pdf, registros, chico, normal, normalBold, chicoBold);
					break;

				case "POR PROVEEDOR":
					TablaPorProveedor(pdf, registros, chico, normal, normalBold, chicoBold);
					break;

				default:
					pdf.Add(new Paragraph("Agrupador no reconocido.", normalBold));
					break;
			}
		}

		private static void TablaSinAgrupar(Document pdf, List<ReporteEvoVtasPerAnterioresDto> registros, Font chico, Font normal, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// ============================
			// Cálculo de totales y rankings
			// ============================
			var total1 = registros.Sum(x => x.vtas_cantidad1);
			var total2 = registros.Sum(x => x.vtas_cantidad2);
			var total3 = registros.Sum(x => x.vtas_cantidad3);

			var lista = registros
				.Select(x => new
				{
					Item = x,
					Porc1 = total1 == 0 ? 0 : (x.vtas_cantidad1 / total1) * 100,
					Porc2 = total2 == 0 ? 0 : (x.vtas_cantidad2 / total2) * 100,
					Porc3 = total3 == 0 ? 0 : (x.vtas_cantidad3 / total3) * 100
				})
				.ToList();

			var rk1 = lista.OrderByDescending(x => x.Porc1)
						   .Select((x, i) => new { x.Item.p_id, RK = i + 1 })
						   .ToDictionary(x => x.p_id, x => x.RK);

			var rk2 = lista.OrderByDescending(x => x.Porc2)
						   .Select((x, i) => new { x.Item.p_id, RK = i + 1 })
						   .ToDictionary(x => x.p_id, x => x.RK);

			var rk3 = lista.OrderByDescending(x => x.Porc3)
						   .Select((x, i) => new { x.Item.p_id, RK = i + 1 })
						   .ToDictionary(x => x.p_id, x => x.RK);

			// ============================
			// Tabla (19 columnas)
			// ============================
			PdfPTable table = new PdfPTable(19)
			{
				WidthPercentage = 100,
				HeaderRows = 2
			};

			float[] widths = {
				25f, // Producto
				4f,5f,6f,10f,8f,10f, // Grupo 1
				4f,5f,6f,10f,8f,10f, // Grupo 2
				4f,5f,6f,10f,8f,10f  // Grupo 3
			};

			table.SetWidths(widths);

			var first = lista.First().Item;

			// ============================
			// Encabezado fila 1
			// ============================
			// Producto con rowspan = 2
			var prodHeader = new PdfPCell(new Phrase("Producto", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray,
				PaddingTop = 4,
				PaddingBottom = 4
			};
			table.AddCell(prodHeader);

			// Colores pastel para los grupos
			var azulPastel = new BaseColor(176, 196, 222); // light steel blue
			var verdePastel = new BaseColor(144, 238, 144); // light green
			var rojoPastel = new BaseColor(255, 182, 193);  // light pink

			AddHeaderGroup(table, first.periodo1.ToString(), 6, normalBold, azulPastel);
			AddHeaderGroup(table, first.periodo2.ToString(), 6, normalBold, verdePastel);
			AddHeaderGroup(table, first.periodo3.ToString(), 6, normalBold, rojoPastel);

			// ============================
			// Encabezado fila 2 (solo columnas de grupos)
			// ============================
			string[] cols = { "RK", "%/Tot.", "Cantidad", "Dif. Año Ant.(C)", "Fact. Neto", "Dif. Año Ant.(F)" };

			foreach (var c in cols) AddHeader(table, c, normalBold, azulPastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, verdePastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, rojoPastel);

			// ============================
			// Filas
			// ============================
			bool alt = false;

			foreach (var x in lista.OrderByDescending(x => x.Porc1))
			{
				var item = x.Item;
				alt = !alt;

				// Producto
				AddCell(table, $"{item.p_desc} ({item.p_id})", chico, alt, Element.ALIGN_LEFT);

				// Grupo 1
				AddCell(table, rk1.GetValueOrDefault(item.p_id).ToString(), chico, alt);
				AddCell(table, x.Porc1.ToString("N2"), chico, alt);
				AddCell(table, item.vtas_cantidad1.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_1_2.ToString("N0"), item.PorcCant_1_2.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion1.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_1_2.ToString("N2"), item.PorcFact_1_2.ToString("N2"), chico, chico, alt));

				// Grupo 2
				AddCell(table, rk2.GetValueOrDefault(item.p_id).ToString(), chico, alt);
				AddCell(table, x.Porc2.ToString("N2"), chico, alt);
				AddCell(table, item.vtas_cantidad2.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_2_3.ToString("N0"), item.PorcCant_2_3.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion2.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_2_3.ToString("N2"), item.PorcFact_2_3.ToString("N2"), chico, chico, alt));

				// Grupo 3
				AddCell(table, rk3.GetValueOrDefault(item.p_id).ToString(), chico, alt);
				AddCell(table, x.Porc3.ToString("N2"), chico, alt);
				AddCell(table, item.vtas_cantidad3.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_3_4.ToString("N0"), item.PorcCant_3_4.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion3.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_3_4.ToString("N2"), item.PorcFact_3_4.ToString("N2"), chico, chico, alt));
			}

			pdf.Add(table);
		}

		private static void TablaPorRubro(Document pdf, List<ReporteEvoVtasPerAnterioresDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// Totales por período
			var totalCant1 = registros.Sum(x => x.vtas_cantidad1);
			var totalCant2 = registros.Sum(x => x.vtas_cantidad2);
			var totalCant3 = registros.Sum(x => x.vtas_cantidad3);

			var totalDifCant1 = registros.Sum(x => x.DifCant_1_2);
			var totalDifCant2 = registros.Sum(x => x.DifCant_2_3);
			var totalDifCant3 = registros.Sum(x => x.DifCant_3_4);

			var totalFact1 = registros.Sum(x => x.vtas_facturacion1);
			var totalFact2 = registros.Sum(x => x.vtas_facturacion2);
			var totalFact3 = registros.Sum(x => x.vtas_facturacion3);

			var totalDifFact1 = registros.Sum(x => x.DifFact_1_2);
			var totalDifFact2 = registros.Sum(x => x.DifFact_2_3);
			var totalDifFact3 = registros.Sum(x => x.DifFact_3_4);

			// Lista con porcentajes
			var lista = registros
				.Select(x => new {
					Item = x,
					Porc1 = totalCant1 == 0 ? 0 : (x.vtas_cantidad1 / totalCant1) * 100,
					Porc2 = totalCant2 == 0 ? 0 : (x.vtas_cantidad2 / totalCant2) * 100,
					Porc3 = totalCant3 == 0 ? 0 : (x.vtas_cantidad3 / totalCant3) * 100
				})
				.OrderBy(x => x.Item.rub_id)
				.ToList();

			// Tabla
			PdfPTable table = new PdfPTable(13)
			{
				WidthPercentage = 100,
				HeaderRows = 2
			};

			float[] widths = {
				27f,
				7f,10f,8f,10f,
				7f,10f,8f,10f,
				7f,10f,8f,10f
			};

			table.SetWidths(widths);

			// Colores pastel
			var azulPastel = new BaseColor(176, 196, 222);
			var verdePastel = new BaseColor(144, 238, 144);
			var rojoPastel = new BaseColor(255, 182, 193);

			// Encabezado fila 1
			var prodHeader = new PdfPCell(new Phrase("Rubro", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(prodHeader);

			var first = lista.First().Item;

			AddHeaderGroup(table, first.periodo1.ToString(), 4, normalBold, azulPastel);
			AddHeaderGroup(table, first.periodo2.ToString(), 4, normalBold, verdePastel);
			AddHeaderGroup(table, first.periodo3.ToString(), 4, normalBold, rojoPastel);

			// Encabezado fila 2
			string[] cols = { "Cantidad", "Dif. Año Ant.(C)", "Fact. Neto", "Dif. Año Ant.(F)" };
			foreach (var c in cols) AddHeader(table, c, normalBold, azulPastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, verdePastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, rojoPastel);

			// Filas
			bool alt = false;

			foreach (var x in lista)
			{
				var item = x.Item;
				alt = !alt;

				AddCell(table, $"{item.rub_desc} ({item.rub_id})", chico, alt, Element.ALIGN_LEFT);

				// Grupo 1
				AddCell(table, item.vtas_cantidad1.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_1_2.ToString("N0"), item.PorcCant_1_2.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion1.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_1_2.ToString("N2"), item.PorcFact_1_2.ToString("N2"), chico, chico, alt));

				// Grupo 2
				AddCell(table, item.vtas_cantidad2.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_2_3.ToString("N0"), item.PorcCant_2_3.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion2.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_2_3.ToString("N2"), item.PorcFact_2_3.ToString("N2"), chico, chico, alt));

				// Grupo 3
				AddCell(table, item.vtas_cantidad3.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_3_4.ToString("N0"), item.PorcCant_3_4.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion3.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_3_4.ToString("N2"), item.PorcFact_3_4.ToString("N2"), chico, chico, alt));
			}

			// ============================
			// FILA TOTALIZADORA
			// ============================
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(tot);

			// Grupo 1
			AddCell(table, totalCant1.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant1.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact1.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact1.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 2
			AddCell(table, totalCant2.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant2.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact2.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact2.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 3
			AddCell(table, totalCant3.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant3.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact3.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact3.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			pdf.Add(table);
		}


		private static void TablaPorSector(Document pdf, List<ReporteEvoVtasPerAnterioresDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// Totales por período
			var totalCant1 = registros.Sum(x => x.vtas_cantidad1);
			var totalCant2 = registros.Sum(x => x.vtas_cantidad2);
			var totalCant3 = registros.Sum(x => x.vtas_cantidad3);

			var totalDifCant1 = registros.Sum(x => x.DifCant_1_2);
			var totalDifCant2 = registros.Sum(x => x.DifCant_2_3);
			var totalDifCant3 = registros.Sum(x => x.DifCant_3_4);

			var totalFact1 = registros.Sum(x => x.vtas_facturacion1);
			var totalFact2 = registros.Sum(x => x.vtas_facturacion2);
			var totalFact3 = registros.Sum(x => x.vtas_facturacion3);

			var totalDifFact1 = registros.Sum(x => x.DifFact_1_2);
			var totalDifFact2 = registros.Sum(x => x.DifFact_2_3);
			var totalDifFact3 = registros.Sum(x => x.DifFact_3_4);

			// Lista con porcentajes
			var lista = registros
				.Select(x => new {
					Item = x,
					Porc1 = totalCant1 == 0 ? 0 : (x.vtas_cantidad1 / totalCant1) * 100,
					Porc2 = totalCant2 == 0 ? 0 : (x.vtas_cantidad2 / totalCant2) * 100,
					Porc3 = totalCant3 == 0 ? 0 : (x.vtas_cantidad3 / totalCant3) * 100
				})
				.OrderBy(x => x.Item.sec_id)
				.ToList();

			// Tabla
			PdfPTable table = new PdfPTable(13)
			{
				WidthPercentage = 100,
				HeaderRows = 2
			};

			float[] widths = {
				21f,
				7f,12f,8f,12f,
				7f,12f,8f,12f,
				7f,12f,8f,12f
			};

			table.SetWidths(widths);

			// Colores pastel
			var azulPastel = new BaseColor(176, 196, 222);
			var verdePastel = new BaseColor(144, 238, 144);
			var rojoPastel = new BaseColor(255, 182, 193);

			// Encabezado fila 1
			var header = new PdfPCell(new Phrase("Sector", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(header);

			var first = lista.First().Item;

			AddHeaderGroup(table, first.periodo1.ToString(), 4, normalBold, azulPastel);
			AddHeaderGroup(table, first.periodo2.ToString(), 4, normalBold, verdePastel);
			AddHeaderGroup(table, first.periodo3.ToString(), 4, normalBold, rojoPastel);

			// Encabezado fila 2
			string[] cols = { "Cantidad", "Dif. Año Ant.(C)", "Fact. Neto", "Dif. Año Ant.(F)" };
			foreach (var c in cols) AddHeader(table, c, normalBold, azulPastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, verdePastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, rojoPastel);

			// Filas
			bool alt = false;

			foreach (var x in lista)
			{
				var item = x.Item;
				alt = !alt;

				AddCell(table, $"{item.sec_desc} ({item.sec_id})", chico, alt, Element.ALIGN_LEFT);

				// Grupo 1
				AddCell(table, item.vtas_cantidad1.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_1_2.ToString("N0"), item.PorcCant_1_2.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion1.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_1_2.ToString("N2"), item.PorcFact_1_2.ToString("N2"), chico, chico, alt));

				// Grupo 2
				AddCell(table, item.vtas_cantidad2.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_2_3.ToString("N0"), item.PorcCant_2_3.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion2.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_2_3.ToString("N2"), item.PorcFact_2_3.ToString("N2"), chico, chico, alt));

				// Grupo 3
				AddCell(table, item.vtas_cantidad3.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_3_4.ToString("N0"), item.PorcCant_3_4.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion3.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_3_4.ToString("N2"), item.PorcFact_3_4.ToString("N2"), chico, chico, alt));
			}

			// ============================
			// FILA TOTALIZADORA
			// ============================
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(tot);

			// Grupo 1
			AddCell(table, totalCant1.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant1.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact1.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact1.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 2
			AddCell(table, totalCant2.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant2.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact2.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact2.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 3
			AddCell(table, totalCant3.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant3.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact3.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact3.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			pdf.Add(table);
		}


		private static void TablaPorProveedor(Document pdf, List<ReporteEvoVtasPerAnterioresDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// Totales por período
			var totalCant1 = registros.Sum(x => x.vtas_cantidad1);
			var totalCant2 = registros.Sum(x => x.vtas_cantidad2);
			var totalCant3 = registros.Sum(x => x.vtas_cantidad3);

			var totalDifCant1 = registros.Sum(x => x.DifCant_1_2);
			var totalDifCant2 = registros.Sum(x => x.DifCant_2_3);
			var totalDifCant3 = registros.Sum(x => x.DifCant_3_4);

			var totalFact1 = registros.Sum(x => x.vtas_facturacion1);
			var totalFact2 = registros.Sum(x => x.vtas_facturacion2);
			var totalFact3 = registros.Sum(x => x.vtas_facturacion3);

			var totalDifFact1 = registros.Sum(x => x.DifFact_1_2);
			var totalDifFact2 = registros.Sum(x => x.DifFact_2_3);
			var totalDifFact3 = registros.Sum(x => x.DifFact_3_4);

			// Lista con porcentajes
			var lista = registros
				.Select(x => new {
					Item = x,
					Porc1 = totalCant1 == 0 ? 0 : (x.vtas_cantidad1 / totalCant1) * 100,
					Porc2 = totalCant2 == 0 ? 0 : (x.vtas_cantidad2 / totalCant2) * 100,
					Porc3 = totalCant3 == 0 ? 0 : (x.vtas_cantidad3 / totalCant3) * 100
				})
				.OrderBy(x => x.Item.cta_id)
				.ToList();

			// Tabla
			PdfPTable table = new PdfPTable(13)
			{
				WidthPercentage = 100,
				HeaderRows = 2
			};

			float[] widths = {
				27f,
				7f,10f,8f,10f,
				7f,10f,8f,10f,
				7f,10f,8f,10f
			};

			table.SetWidths(widths);

			// Colores pastel
			var azulPastel = new BaseColor(176, 196, 222);
			var verdePastel = new BaseColor(144, 238, 144);
			var rojoPastel = new BaseColor(255, 182, 193);

			// Encabezado fila 1
			var header = new PdfPCell(new Phrase("Cuenta", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(header);

			var first = lista.First().Item;

			AddHeaderGroup(table, first.periodo1.ToString(), 4, normalBold, azulPastel);
			AddHeaderGroup(table, first.periodo2.ToString(), 4, normalBold, verdePastel);
			AddHeaderGroup(table, first.periodo3.ToString(), 4, normalBold, rojoPastel);

			// Encabezado fila 2
			string[] cols = { "Cantidad", "Dif. Año Ant.(C)", "Fact. Neto", "Dif. Año Ant.(F)" };
			foreach (var c in cols) AddHeader(table, c, normalBold, azulPastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, verdePastel);
			foreach (var c in cols) AddHeader(table, c, normalBold, rojoPastel);

			// Filas
			bool alt = false;

			foreach (var x in lista)
			{
				var item = x.Item;
				alt = !alt;

				AddCell(table, $"{item.cta_denominacion} ({item.cta_id})", chico, alt, Element.ALIGN_LEFT);

				// Grupo 1
				AddCell(table, item.vtas_cantidad1.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_1_2.ToString("N0"), item.PorcCant_1_2.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion1.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_1_2.ToString("N2"), item.PorcFact_1_2.ToString("N2"), chico, chico, alt));

				// Grupo 2
				AddCell(table, item.vtas_cantidad2.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_2_3.ToString("N0"), item.PorcCant_2_3.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion2.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_2_3.ToString("N2"), item.PorcFact_2_3.ToString("N2"), chico, chico, alt));

				// Grupo 3
				AddCell(table, item.vtas_cantidad3.ToString("N0"), chico, alt);
				table.AddCell(CeldaDoble(item.DifCant_3_4.ToString("N0"), item.PorcCant_3_4.ToString("N2"), chico, chico, alt));
				AddCell(table, item.vtas_facturacion3.ToString("N2"), chico, alt);
				table.AddCell(CeldaDoble(item.DifFact_3_4.ToString("N2"), item.PorcFact_3_4.ToString("N2"), chico, chico, alt));
			}

			// ============================
			// FILA TOTALIZADORA
			// ============================
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LightGray
			};
			table.AddCell(tot);

			// Grupo 1
			AddCell(table, totalCant1.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant1.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact1.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact1.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 2
			AddCell(table, totalCant2.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant2.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact2.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact2.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			// Grupo 3
			AddCell(table, totalCant3.ToString("N0"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifCant3.ToString("N0"), "0.00", chicoBold, chicoBold, false));
			AddCell(table, totalFact3.ToString("N2"), chicoBold, false);
			table.AddCell(CeldaDoble(totalDifFact3.ToString("N2"), "0.00", chicoBold, chicoBold, false));

			pdf.Add(table);
		}



		// ============================================================
		// HELPERS
		// ============================================================
		private static PdfPCell CeldaDoble(string izquierda, string derecha, Font fontIzq, Font fontDer, bool alt)
		{
			// Formateo seguro a 2 decimales
			if (decimal.TryParse(izquierda, out var izqVal))
				izquierda = izqVal.ToString("N2");

			if (decimal.TryParse(derecha, out var derVal))
				derecha = derVal.ToString("N2");

			// Tabla interna de 2 columnas en una sola fila
			PdfPTable inner = new PdfPTable(2);
			inner.WidthPercentage = 100;
			inner.SetWidths(new float[] { 1f, 1f });

			// Celda izquierda
			PdfPCell c1 = new PdfPCell(new Phrase(izquierda, fontIzq))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				Padding = 0
			};

			// Celda derecha con fondo gris
			PdfPCell c2 = new PdfPCell(new Phrase(derecha, fontDer))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = new BaseColor(230, 230, 230),
				Padding = 0
			};

			inner.AddCell(c1);
			inner.AddCell(c2);

			// Celda contenedora
			PdfPCell celda = new PdfPCell(inner)
			{
				Padding = 2
			};

			if (alt)
				celda.BackgroundColor = new BaseColor(245, 245, 245);

			return celda;
		}


		private static void AddHeaderGroup(PdfPTable table, string texto, int colspan, Font font, BaseColor color)
		{
			var cell = new PdfPCell(new Phrase(texto, font))
			{
				Colspan = colspan,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = color,
				PaddingTop = 4,
				PaddingBottom = 4
			};
			table.AddCell(cell);
		}

		private static void AddHeader(PdfPTable table, string texto, Font font, BaseColor color)
		{
			var cell = new PdfPCell(new Phrase(texto, font))
			{
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = color,
				PaddingTop = 3,
				PaddingBottom = 3
			};
			table.AddCell(cell);
		}



		private static void AddRow(PdfPTable table, ReporteEvoVtasPerAnterioresDto item, Font chico, bool alt, bool useRubros = false, bool useSector = false, bool useProveedor = false)
		{
			string id = item.p_id;
			string desc = item.p_desc;

			if (useRubros)
			{
				id = item.rub_id;
				desc = item.rub_desc;
			}
			else if (useSector)
			{
				id = item.sec_id;
				desc = item.sec_desc;
			}
			else if (useProveedor)
			{
				id = item.cta_id;
				desc = item.cta_denominacion;
			}

			AddCell(table, id, chico, alt, Element.ALIGN_CENTER);
			AddCell(table, desc, chico, alt, Element.ALIGN_LEFT);

			AddCell(table, item.vtas_cantidad1.ToString("N0"), chico, alt);
			AddCell(table, item.DifCant_1_2.ToString("N0"), chico, alt);
			AddCell(table, item.PorcCant_1_2.ToString("N2"), chico, alt);

			AddCell(table, item.vtas_facturacion1.ToString("N2"), chico, alt);
			AddCell(table, item.DifFact_1_2.ToString("N2"), chico, alt);
			AddCell(table, item.PorcFact_1_2.ToString("N2"), chico, alt);

			AddCell(table, item.vtas_cantidad2.ToString("N0"), chico, alt);
			AddCell(table, item.DifCant_2_3.ToString("N0"), chico, alt);
			AddCell(table, item.PorcCant_2_3.ToString("N2"), chico, alt);

			AddCell(table, item.vtas_facturacion2.ToString("N2"), chico, alt);
			AddCell(table, item.DifFact_2_3.ToString("N2"), chico, alt);
			AddCell(table, item.PorcFact_2_3.ToString("N2"), chico, alt);

			AddCell(table, item.vtas_cantidad3.ToString("N0"), chico, alt);
			AddCell(table, item.DifCant_3_4.ToString("N0"), chico, alt);
			AddCell(table, item.PorcCant_3_4.ToString("N2"), chico, alt);

			AddCell(table, item.vtas_facturacion3.ToString("N2"), chico, alt);
			AddCell(table, item.DifFact_3_4.ToString("N2"), chico, alt);
			AddCell(table, item.PorcFact_3_4.ToString("N2"), chico, alt);
		}

		private static void AddCell(PdfPTable table, string texto, Font font, bool alt, int align = Element.ALIGN_RIGHT)
		{
			var cell = new PdfPCell(new Phrase(texto, font))
			{
				HorizontalAlignment = align
			};

			if (alt)
				cell.BackgroundColor = new BaseColor(245, 245, 245);

			table.AddCell(cell);
		}

		#endregion

		private List<ReporteEvoVtasPerAnterioresDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string tipoReporte, out string filtrosString)
		{
			try
			{
				var ret = new List<ReporteEvoVtasPerAnterioresDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				var lSuc_temp = solicitud.Parametros.GetValueOrDefault("lSuc", "")?.ToString() ?? null;
				List<string> lSuc = string.IsNullOrWhiteSpace(lSuc_temp) ? [] : [.. lSuc_temp.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))];
				var lProv_temp = solicitud.Parametros.GetValueOrDefault("lProv", "")?.ToString() ?? null;
				List<string> lProv = string.IsNullOrWhiteSpace(lProv_temp) ? [] : [.. lProv_temp.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))];
				var lFam_temp = solicitud.Parametros.GetValueOrDefault("lFam", "")?.ToString() ?? null;
				List<string> lFam = string.IsNullOrWhiteSpace(lFam_temp) ? [] : [.. lFam_temp.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))];
				var lRub_temp = solicitud.Parametros.GetValueOrDefault("lRub", "")?.ToString() ?? null;
				List<string> lRub = string.IsNullOrWhiteSpace(lRub_temp) ? [] : [.. lRub_temp.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))];
				var agrupador = solicitud.Parametros.GetValueOrDefault("agrupador", "")?.ToString() ?? null;
				tipoReporte = solicitud.Parametros.GetValueOrDefault("tipoReporte", "")?.ToString() ?? null;
				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;
				ret = _consSrv.RepEvoVtasPerAnteriores(new ReporteEvoVtasPerAnterioresRequest()
				{
					agrupador = Convert.ToInt32(agrupador),
					desde=desde,
					hasta = hasta,
					lFam = lFam,
					lProv=lProv,
					lRub=lRub,
					lSuc=lSuc
				});
				titulo = $"Reporte Evo. de Vtas. para Per. Anteriores {tipoReporte}";
				subtitulo = $"Desde: {desde.ToString("dd/MM/yyyy")} Hasta: {hasta.ToString("dd/MM/yyyy")}\n{filtrosString}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtitulo = "";
				tipoReporte = "";
				filtrosString = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			string tipoReporte;
			string filtrosString;
			List<ReporteEvoVtasPerAnterioresDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
			string tipoReporte;
			string filtrosString;
			List<ReporteEvoVtasPerAnterioresDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
