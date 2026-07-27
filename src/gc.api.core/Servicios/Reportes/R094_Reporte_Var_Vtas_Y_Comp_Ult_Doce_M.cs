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
				List<ReporteVarVtasYCompUltDoceMDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
				CargarRepoVarVtasYCompUltDoceM(pdf, registros, filtrosString, tipoReporte, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoVarVtasYCompUltDoceM(Document pdf, List<ReporteVarVtasYCompUltDoceMDto> registros, string filtrosString, string tipoReporte, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay datos para mostrar.", normal));
				return;
			}

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

		private static void TablaSinAgrupar(Document pdf, List<ReporteVarVtasYCompUltDoceMDto> registros, Font chico, Font normal, Font normalBold)
		{
			var lista = registros.OrderBy(x => x.rub_id).ToList();
			PdfPTable tabla = CrearTablaBase(chico, normalBold);

			AgregarEncabezado(tabla, lista, normalBold);

			string grupoAnterior = null;

			foreach (var item in lista)
			{
				string grupoActual = item.rub_id;
				string leyenda = $"({item.rub_id}) {item.rub_desc}";

				if (grupoActual != grupoAnterior)
				{
					PdfPCell agrup = new PdfPCell(new Phrase(leyenda, normalBold));
					agrup.Colspan = 26;
					agrup.BackgroundColor = new BaseColor(220, 220, 220);
					agrup.HorizontalAlignment = Element.ALIGN_CENTER;
					tabla.AddCell(agrup);

					grupoAnterior = grupoActual;
				}

				tabla.AddCell(new PdfPCell(new Phrase(item.p_id.ToString(), chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(item.p_desc, chico)));

				for (int i = 1; i <= 12; i++)
				{
					var vtaCant = (int)item.GetType().GetProperty($"vta_cantidad{i}")?.GetValue(item);
					var compCant = (int)item.GetType().GetProperty($"comp_cantidad{i}")?.GetValue(item);
					var vtaFact = (decimal)item.GetType().GetProperty($"vta_neto{i}")?.GetValue(item);
					var compFact = (decimal)item.GetType().GetProperty($"comp_costo{i}")?.GetValue(item);

					tabla.AddCell(CeldaDoble(vtaCant, compCant, chico));
					tabla.AddCell(CeldaDoble(vtaFact, compFact, chico));
				}
			}

			pdf.Add(tabla);
		}


		private static void TablaPorRubro(Document pdf, List<ReporteVarVtasYCompUltDoceMDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			var lista = registros.OrderBy(x => x.sec_id).ToList();
			PdfPTable tabla = CrearTablaBase(chico, normalBold);

			AgregarEncabezado(tabla, lista, normalBold);

			string grupoAnterior = null;

			foreach (var item in lista)
			{
				string grupoActual = item.sec_id;
				string leyenda = $"({item.sec_id}) {item.sec_desc}";

				if (grupoActual != grupoAnterior)
				{
					PdfPCell agrup = new PdfPCell(new Phrase(leyenda, chicoBold));
					agrup.Colspan = 26;
					agrup.BackgroundColor = new BaseColor(220, 220, 220);
					agrup.HorizontalAlignment = Element.ALIGN_CENTER;
					tabla.AddCell(agrup);

					grupoAnterior = grupoActual;
				}

				tabla.AddCell(new PdfPCell(new Phrase(item.rub_id.ToString(), chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(item.rub_desc, chico)));

				for (int i = 1; i <= 12; i++)
				{
					var vtaCant = (int)item.GetType().GetProperty($"vta_cantidad{i}")?.GetValue(item);
					var compCant = (int)item.GetType().GetProperty($"comp_cantidad{i}")?.GetValue(item);
					var vtaFact = (decimal)item.GetType().GetProperty($"vta_neto{i}")?.GetValue(item);
					var compFact = (decimal)item.GetType().GetProperty($"comp_costo{i}")?.GetValue(item);

					tabla.AddCell(CeldaDoble(vtaCant, compCant, chico));
					tabla.AddCell(CeldaDoble(vtaFact, compFact, chico));
				}
			}

			pdf.Add(tabla);
		}


		private static void TablaPorSector(Document pdf, List<ReporteVarVtasYCompUltDoceMDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			var lista = registros.OrderBy(x => x.sec_id).ToList();
			PdfPTable tabla = CrearTablaBase(chico, normalBold);

			AgregarEncabezado(tabla, lista, normalBold);

			foreach (var item in lista)
			{
				tabla.AddCell(new PdfPCell(new Phrase(item.sec_id.ToString(), chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(item.sec_desc, chico)));

				for (int i = 1; i <= 12; i++)
				{
					var vtaCant = (int)item.GetType().GetProperty($"vta_cantidad{i}")?.GetValue(item);
					var compCant = (int)item.GetType().GetProperty($"comp_cantidad{i}")?.GetValue(item);
					var vtaFact = (decimal)item.GetType().GetProperty($"vta_neto{i}")?.GetValue(item);
					var compFact = (decimal)item.GetType().GetProperty($"comp_costo{i}")?.GetValue(item);

					tabla.AddCell(CeldaDoble(vtaCant, compCant, chico));
					tabla.AddCell(CeldaDoble(vtaFact, compFact, chico));
				}
			}

			pdf.Add(tabla);
		}


		private static void TablaPorProveedor(Document pdf, List<ReporteVarVtasYCompUltDoceMDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			var lista = registros.OrderBy(x => x.cta_id).ToList();
			PdfPTable tabla = CrearTablaBase(chico, normalBold);

			AgregarEncabezado(tabla, lista, normalBold);

			foreach (var item in lista)
			{
				tabla.AddCell(new PdfPCell(new Phrase(item.cta_id.ToString(), chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(item.cta_denominacion, chico)));

				for (int i = 1; i <= 12; i++)
				{
					var vtaCant = (int)item.GetType().GetProperty($"vta_cantidad{i}")?.GetValue(item);
					var compCant = (int)item.GetType().GetProperty($"comp_cantidad{i}")?.GetValue(item);
					var vtaFact = (decimal)item.GetType().GetProperty($"vta_neto{i}")?.GetValue(item);
					var compFact = (decimal)item.GetType().GetProperty($"comp_costo{i}")?.GetValue(item);

					tabla.AddCell(CeldaDoble(vtaCant, compCant, chico));
					tabla.AddCell(CeldaDoble(vtaFact, compFact, chico));
				}
			}

			pdf.Add(tabla);
		}


		// ============================================================
		// HELPERS
		// ============================================================
		private static PdfPTable CrearTablaBase(Font chico, Font normalBold)
		{
			// 26 columnas: ID + DESC + (12 períodos × 2 columnas)
			PdfPTable tabla = new PdfPTable(26);
			tabla.WidthPercentage = 100;

			float[] widths = new float[26];
			widths[0] = 3f;   // ID
			widths[1] = 8f;   // Descripción

			for (int i = 2; i < 26; i++)
				widths[i] = 3.5f; // Cant / Fact

			tabla.SetWidths(widths);
			return tabla;
		}

		private static void AgregarEncabezado(PdfPTable tabla, List<ReporteVarVtasYCompUltDoceMDto> registros, Font normalBold)
		{
			var first = registros.First();

			// Fila 1
			PdfPCell cId = new PdfPCell(new Phrase("ID", normalBold));
			cId.Rowspan = 2;
			cId.HorizontalAlignment = Element.ALIGN_CENTER;
			cId.VerticalAlignment = Element.ALIGN_MIDDLE;
			cId.BackgroundColor = new BaseColor(180, 180, 180);
			tabla.AddCell(cId);

			PdfPCell cDesc = new PdfPCell(new Phrase("Descripción", normalBold));
			cDesc.Rowspan = 2;
			cDesc.HorizontalAlignment = Element.ALIGN_CENTER;
			cDesc.VerticalAlignment = Element.ALIGN_MIDDLE;
			cDesc.BackgroundColor = new BaseColor(180, 180, 180);
			tabla.AddCell(cDesc);

			for (int i = 1; i <= 12; i++)
			{
				var periodoProp = first.GetType().GetProperty($"periodo{i}");
				var periodoRaw = periodoProp?.GetValue(first)?.ToString() ?? $"Periodo {i}";
				string periodo = periodoRaw.Length == 6
					? $"{periodoRaw.Substring(0, 4)}-{periodoRaw.Substring(4, 2)}"
					: periodoRaw;

				PdfPCell celdaPeriodo = new PdfPCell(new Phrase(periodo, normalBold));
				celdaPeriodo.Colspan = 2;
				celdaPeriodo.HorizontalAlignment = Element.ALIGN_CENTER;
				celdaPeriodo.BackgroundColor = new BaseColor(180, 180, 180);
				tabla.AddCell(celdaPeriodo);
			}

			// Fila 2
			for (int i = 1; i <= 12; i++)
			{
				tabla.AddCell(new PdfPCell(new Phrase("Cant.", normalBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = new BaseColor(180, 180, 180) });
				tabla.AddCell(new PdfPCell(new Phrase("Fact.", normalBold)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = new BaseColor(180, 180, 180) });
			}
		}

		private static PdfPCell CeldaDoble(object vta, object comp, Font chico)
		{
			Phrase ph = new Phrase();
			ph.Add(new Chunk($"{vta}\n", chico));
			ph.Add(new Chunk($"{comp}", chico));

			PdfPCell celda = new PdfPCell(ph);
			celda.HorizontalAlignment = Element.ALIGN_RIGHT;
			celda.Padding = 2f;
			return celda;
		}


		#endregion

		private List<ReporteVarVtasYCompUltDoceMDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string tipoReporte, out string filtrosString)
		{
			try
			{
				var ret = new List<ReporteVarVtasYCompUltDoceMDto>();
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
				ret = _consSrv.RepoVarVtasYCompUltDoceM(new ReporteVarVtasYCompUltDoceMRequest()
				{
					agrupador = Convert.ToInt32(agrupador),
					lFam = lFam,
					lProv=lProv,
					lRub=lRub,
					lSuc=lSuc
				});
				titulo = $"Reporte Variación de Vtas. y Comp. Últ. 12 meses {tipoReporte}";
				subtitulo = $"{filtrosString}";
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
			List<ReporteVarVtasYCompUltDoceMDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
			List<ReporteVarVtasYCompUltDoceMDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
