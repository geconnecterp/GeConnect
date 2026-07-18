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
	public class R092_Reporte_Ranking_Y_Rentabilidad : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R092_Reporte_Ranking_Y_Rentabilidad(IUnitOfWork uow, IConsultaServicio consSrv,
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
				List<RepRkgRentabVtasDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
				CargarComisionDeRepartidoresResumen(pdf, registros, filtrosString, tipoReporte, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarComisionDeRepartidoresResumen(Document pdf, List<RepRkgRentabVtasDto> registros, string filtrosString, string tipoReporte, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

			// ============================
			// 3) SELECCIÓN DE TABLA SEGÚN AGRUPADOR
			// ============================
			switch (tipoReporte.ToUpper())
			{
				case "SIN AGRUPAR":
					TablaSinAgrupar(pdf, registros, chico, normal, normalBold);
					break;

				case "POR RUBROS":
					TablaPorRubro(pdf, registros, chico, normal, normalBold);
					break;

				case "POR PROVEEDOR":
					TablaPorProveedor(pdf, registros, chico, normal, normalBold);
					break;

				case "POR SECTOR":
					TablaPorSector(pdf, registros, chico, normal, normalBold);
					break;

				default:
					pdf.Add(new Paragraph("Agrupador no reconocido.", normalBold));
					break;
			}
		}

		private static void TablaSinAgrupar(Document pdf, List<RepRkgRentabVtasDto> registros, Font chico, Font normal, Font normalBold)
		{
			PdfPTable table = new(11)
			{
				WidthPercentage = 100,
				HeaderRows = 1
			};

			// Ajuste fino de columnas
			float[] widths = {
				6f,   // ID
				30f,  // Descripción
				6f,   // Cant. Vend.
				7f,   // %Cant/Total
				9f,  // Facturado
				6f,   // %Fac/Total
				8f,  // Ventas Netas
				8f,  // Costo
				8f,  // Rentabilidad
				5f,   // %Rent/Total
				5f    // %Rent/Costo
			};

			table.SetWidths(widths);

			string[] headers = {
				"ID","Descripción","Cant. Vend.","%Cant/Total","Facturado",
				"%Fac/Total","Ventas Netas","Costo","Rentabilidad",
				"%Rent/Total","%Rent/Costo"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(200, 200, 200), // gris más oscuro
					PaddingTop = 4,
					PaddingBottom = 4
				};

				table.AddCell(celda);
			}

			string grupoAnterior = null;
			bool alt = false;

			// Totales
			decimal totalCant = 0;
			decimal totalCantPorc = 0;
			decimal totalFact = 0;
			decimal totalFactPorc = 0;
			decimal totalRent = 0;

			foreach (var item in registros.OrderBy(x => x.rub_id))
			{
				// Fila de agrupación por Rubro
				if (item.rub_id != grupoAnterior)
				{
					PdfPCell grp = new PdfPCell(new Phrase($"({item.rub_id}) {item.rub_desc}", normalBold));
					grp.Colspan = 11;
					grp.HorizontalAlignment = Element.ALIGN_CENTER;
					grp.BackgroundColor = BaseColor.LightGray;
					table.AddCell(grp);

					grupoAnterior = item.rub_id;
					alt = false; // reinicia alternado
				}

				// Alternado
				alt = !alt;

				table.AddCell(CeldaCentrada(item.p_id, chico, alt));
				table.AddCell(CeldaIzq(item.p_desc, chico, alt));
				table.AddCell(Celda(item.vtas_cantidad.ToString("N0"), chico, alt));
				table.AddCell(Celda(item.vtas_cantidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_neto.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_costo.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_renta_costo_porc.ToString("N2"), chico, alt));

				// Acumular totales
				totalCant += item.vtas_cantidad;
				totalCantPorc += item.vtas_cantidad_porc;
				totalFact += item.vtas_facturacion;
				totalFactPorc += item.vtas_facturacion_porc;
				totalRent += item.vtas_rentabilidad;
			}

			// ============================
			// FILA DE TOTALES GENERALES
			// ============================

			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold));
			tot.Colspan = 2; // ID + Descripción
			tot.HorizontalAlignment = Element.ALIGN_RIGHT;
			tot.BackgroundColor = BaseColor.LightGray;
			table.AddCell(tot);

			table.AddCell(CeldaTotal(totalCant.ToString("N0"), normalBold));
			table.AddCell(CeldaTotal(totalCantPorc.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFact.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFactPorc.ToString("N2"), normalBold));

			// Ventas netas y costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			table.AddCell(CeldaTotal(totalRent.ToString("N2"), normalBold));

			// %Rent/Total y %Rent/Costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			pdf.Add(table);
		}

		private static void TablaPorRubro(Document pdf, List<RepRkgRentabVtasDto> registros, Font chico, Font normal, Font normalBold)
		{
			PdfPTable table = new PdfPTable(11)
			{
				WidthPercentage = 100,
				HeaderRows = 1
			};

			// Ajuste fino de columnas
			float[] widths = {
				6f,   // ID
				30f,  // Descripción
				6f,   // Cant. Vend.
				7f,   // %Cant/Total
				9f,  // Facturado
				6f,   // %Fac/Total
				8f,  // Ventas Netas
				8f,  // Costo
				8f,  // Rentabilidad
				5f,   // %Rent/Total
				5f    // %Rent/Costo
			};

			table.SetWidths(widths);

			string[] headers = {
				"ID","Rubro","Cant. Vend.","%Cant/Total","Facturado",
				"%Fac/Total","Ventas Netas","Costo","Rentabilidad",
				"%Rent/Total","%Rent/Costo"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(200, 200, 200), // gris más oscuro
					PaddingTop = 4,
					PaddingBottom = 4
				};

				table.AddCell(celda);
			}

			bool alt = false;

			decimal totalCant = 0;
			decimal totalCantPorc = 0;
			decimal totalFact = 0;
			decimal totalFactPorc = 0;
			decimal totalRent = 0;

			foreach (var item in registros.OrderBy(x => x.rub_id))
			{
				alt = !alt;

				table.AddCell(CeldaCentrada(item.rub_id, chico, alt));
				table.AddCell(CeldaIzq(item.rub_desc, chico, alt));
				table.AddCell(Celda(item.vtas_cantidad.ToString("N0"), chico, alt));
				table.AddCell(Celda(item.vtas_cantidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_neto.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_costo.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_renta_costo_porc.ToString("N2"), chico, alt));

				totalCant += item.vtas_cantidad;
				totalCantPorc += item.vtas_cantidad_porc;
				totalFact += item.vtas_facturacion;
				totalFactPorc += item.vtas_facturacion_porc;
				totalRent += item.vtas_rentabilidad;
			}

			// Totales
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold));
			tot.Colspan = 2;
			tot.HorizontalAlignment = Element.ALIGN_RIGHT;
			tot.BackgroundColor = BaseColor.LightGray;
			table.AddCell(tot);

			table.AddCell(CeldaTotal(totalCant.ToString("N0"), normalBold));
			table.AddCell(CeldaTotal(totalCantPorc.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFact.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFactPorc.ToString("N2"), normalBold));

			// Ventas netas y costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			table.AddCell(CeldaTotal(totalRent.ToString("N2"), normalBold));

			// %Rent/Total y %Rent/Costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			pdf.Add(table);
		}

		private static void TablaPorProveedor(Document pdf, List<RepRkgRentabVtasDto> registros, Font chico, Font normal, Font normalBold)
		{
			PdfPTable table = new PdfPTable(11)
			{
				WidthPercentage = 100,
				HeaderRows = 1
			};

			// Ajuste fino de columnas
			float[] widths = {
				6f,   // ID
				30f,  // Descripción
				6f,   // Cant. Vend.
				7f,   // %Cant/Total
				9f,  // Facturado
				6f,   // %Fac/Total
				8f,  // Ventas Netas
				8f,  // Costo
				8f,  // Rentabilidad
				5f,   // %Rent/Total
				5f    // %Rent/Costo
			};

			table.SetWidths(widths);

			string[] headers = {
				"ID","Cuenta","Cant. Vend.","%Cant/Total","Facturado",
				"%Fac/Total","Ventas Netas","Costo","Rentabilidad",
				"%Rent/Total","%Rent/Costo"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(200, 200, 200), // gris más oscuro
					PaddingTop = 4,
					PaddingBottom = 4
				};

				table.AddCell(celda);
			}

			bool alt = false;

			decimal totalCant = 0;
			decimal totalCantPorc = 0;
			decimal totalFact = 0;
			decimal totalFactPorc = 0;
			decimal totalRent = 0;

			foreach (var item in registros.OrderBy(x => x.cta_id))
			{
				alt = !alt;

				table.AddCell(CeldaCentrada(item.cta_id, chico, alt));
				table.AddCell(CeldaIzq(item.cta_denominacion, chico, alt));
				table.AddCell(Celda(item.vtas_cantidad.ToString("N0"), chico, alt));
				table.AddCell(Celda(item.vtas_cantidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_neto.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_costo.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_renta_costo_porc.ToString("N2"), chico, alt));

				totalCant += item.vtas_cantidad;
				totalCantPorc += item.vtas_cantidad_porc;
				totalFact += item.vtas_facturacion;
				totalFactPorc += item.vtas_facturacion_porc;
				totalRent += item.vtas_rentabilidad;
			}

			// Totales
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold));
			tot.Colspan = 2;
			tot.HorizontalAlignment = Element.ALIGN_RIGHT;
			tot.BackgroundColor = BaseColor.LightGray;
			table.AddCell(tot);

			table.AddCell(CeldaTotal(totalCant.ToString("N0"), normalBold));
			table.AddCell(CeldaTotal(totalCantPorc.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFact.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFactPorc.ToString("N2"), normalBold));

			// Ventas netas y costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			table.AddCell(CeldaTotal(totalRent.ToString("N2"), normalBold));

			// %Rent/Total y %Rent/Costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			pdf.Add(table);
		}

		private static void TablaPorSector(Document pdf, List<RepRkgRentabVtasDto> registros, Font chico, Font normal, Font normalBold)
		{
			PdfPTable table = new PdfPTable(11)
			{
				WidthPercentage = 100,
				HeaderRows = 1
			};

			// Ajuste fino de columnas
			float[] widths = {
				6f,   // ID
				30f,  // Descripción
				6f,   // Cant. Vend.
				7f,   // %Cant/Total
				9f,  // Facturado
				6f,   // %Fac/Total
				8f,  // Ventas Netas
				8f,  // Costo
				8f,  // Rentabilidad
				5f,   // %Rent/Total
				5f    // %Rent/Costo
			};

			table.SetWidths(widths);

			string[] headers = {
				"ID","Sector","Cant. Vend.","%Cant/Total","Facturado",
				"%Fac/Total","Ventas Netas","Costo","Rentabilidad",
				"%Rent/Total","%Rent/Costo"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(200, 200, 200), // gris más oscuro
					PaddingTop = 4,
					PaddingBottom = 4
				};

				table.AddCell(celda);
			}

			bool alt = false;

			decimal totalCant = 0;
			decimal totalCantPorc = 0;
			decimal totalFact = 0;
			decimal totalFactPorc = 0;
			decimal totalRent = 0;

			foreach (var item in registros.OrderBy(x => x.sec_id))
			{
				alt = !alt;

				table.AddCell(CeldaCentrada(item.sec_id, chico, alt));
				table.AddCell(CeldaIzq(item.sec_desc, chico, alt));
				table.AddCell(Celda(item.vtas_cantidad.ToString("N0"), chico, alt));
				table.AddCell(Celda(item.vtas_cantidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_facturacion_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_neto.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_costo.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_rentabilidad_porc.ToString("N2"), chico, alt));
				table.AddCell(Celda(item.vtas_renta_costo_porc.ToString("N2"), chico, alt));

				totalCant += item.vtas_cantidad;
				totalCantPorc += item.vtas_cantidad_porc;
				totalFact += item.vtas_facturacion;
				totalFactPorc += item.vtas_facturacion_porc;
				totalRent += item.vtas_rentabilidad;
			}

			// Totales
			PdfPCell tot = new PdfPCell(new Phrase("TOTALES", normalBold));
			tot.Colspan = 2;
			tot.HorizontalAlignment = Element.ALIGN_RIGHT;
			tot.BackgroundColor = BaseColor.LightGray;
			table.AddCell(tot);

			table.AddCell(CeldaTotal(totalCant.ToString("N0"), normalBold));
			table.AddCell(CeldaTotal(totalCantPorc.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFact.ToString("N2"), normalBold));
			table.AddCell(CeldaTotal(totalFactPorc.ToString("N2"), normalBold));

			// Ventas netas y costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			table.AddCell(CeldaTotal(totalRent.ToString("N2"), normalBold));

			// %Rent/Total y %Rent/Costo no se totalizan
			table.AddCell(CeldaTotal("", normalBold));
			table.AddCell(CeldaTotal("", normalBold));

			pdf.Add(table);
		}

		// ============================================================
		// HELPERS
		// ============================================================

		private static PdfPCell Celda(string texto, Font font, bool alt)
		{
			var cell = new PdfPCell(new Phrase(texto, font));
			cell.HorizontalAlignment = Element.ALIGN_RIGHT;

			if (alt)
				cell.BackgroundColor = new BaseColor(240, 240, 240); // gris suave

			return cell;
		}

		private static PdfPCell CeldaCentrada(string texto, Font font, bool alt)
		{
			var cell = new PdfPCell(new Phrase(texto, font));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;

			if (alt)
				cell.BackgroundColor = new BaseColor(240, 240, 240);

			return cell;
		}

		private static PdfPCell CeldaIzq(string texto, Font font, bool alt)
		{
			var cell = new PdfPCell(new Phrase(texto, font));
			cell.HorizontalAlignment = Element.ALIGN_LEFT;

			if (alt)
				cell.BackgroundColor = new BaseColor(240, 240, 240);

			return cell;
		}

		private static PdfPCell CeldaTotal(string texto, Font font)
		{
			return new PdfPCell(new Phrase(texto, font))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LightGray,
				PaddingTop = 4,
				PaddingBottom = 4
			};
		}

		#endregion

		private List<RepRkgRentabVtasDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string tipoReporte, out string filtrosString)
		{
			try
			{
				var ret = new List<RepRkgRentabVtasDto>();
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
				ret = _consSrv.RepRkgRentabVtas(new ReporteRankingRentabVtasRequest()
				{
					agrupador = Convert.ToInt32(agrupador),
					desde=desde,
					hasta = hasta,
					lFam = lFam,
					lProv=lProv,
					lRub=lRub,
					lSuc=lSuc
				});
				titulo = $"Informe de Ventas {tipoReporte}";
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
			List<RepRkgRentabVtasDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
			List<RepRkgRentabVtasDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
