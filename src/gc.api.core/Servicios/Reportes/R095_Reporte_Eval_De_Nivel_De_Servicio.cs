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
	public class R095_Reporte_Eval_De_Nivel_De_Servicio : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R095_Reporte_Eval_De_Nivel_De_Servicio(IUnitOfWork uow, IConsultaServicio consSrv,
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
				List<ReporteEvalDeNivelDeServicioDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
				CargarRepoEvalDeNivelDeServicio(pdf, registros, filtrosString, tipoReporte, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoEvalDeNivelDeServicio(Document pdf, List<ReporteEvalDeNivelDeServicioDto> registros, string filtrosString, string tipoReporte, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
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

		private static void TablaSinAgrupar(Document pdf, List<ReporteEvalDeNivelDeServicioDto> registros, Font chico, Font normal, Font normalBold)
		{
			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 5, 60, 7, 7, 10, 10, 10, 10 });

			// Encabezados
			AgregarCeldaHeader(tabla, "ID", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Alta Rot.", normalBold);
			AgregarCeldaHeader(tabla, "Stock", normalBold);
			AgregarCeldaHeader(tabla, "Ult. Comp.", normalBold);
			AgregarCeldaHeader(tabla, "Ult. Rtr.", normalBold);
			AgregarCeldaHeader(tabla, "Vta. Ult. 30 d.", normalBold);
			AgregarCeldaHeader(tabla, "N. de Serv.", normalBold);

			// 🔥 ESTA LÍNEA ES LA CLAVE
			tabla.HeaderRows = 1;

			string grupoAnterior = "";
			foreach (var item in registros.OrderBy(x => x.rub_id))
			{
				string grupoActual = item.rub_id;
				string leyenda = $"({item.rub_id}) {item.rub_desc}";

				if (grupoActual != grupoAnterior)
				{
					PdfPCell celdaGrupo = new PdfPCell(new Phrase(leyenda, normalBold));
					celdaGrupo.Colspan = 8;
					celdaGrupo.HorizontalAlignment = Element.ALIGN_CENTER;
					celdaGrupo.BackgroundColor = BaseColor.LightGray;
					tabla.AddCell(celdaGrupo);

					grupoAnterior = grupoActual;
				}

				// ID → centrado
				tabla.AddCell(Celda(item.p_id, chico, Element.ALIGN_CENTER));

				// Descripción → izquierda (default)
				tabla.AddCell(Celda(item.p_desc, chico, Element.ALIGN_LEFT));

				// Alta Rotación → centrado
				tabla.AddCell(Celda(item.p_alta_rotacion == 'S' ? "SI" : "NO", chico, Element.ALIGN_CENTER));

				// Stock → derecha
				tabla.AddCell(Celda(item.stk.ToString("N2"), chico, Element.ALIGN_RIGHT));

				// Último comprobante → centrado
				tabla.AddCell(Celda(item.rp_fecha?.ToShortDateString() ?? "", chico, Element.ALIGN_CENTER));

				// Último retiro → centrado
				tabla.AddCell(Celda(item.re_fecha?.ToShortDateString() ?? "", chico, Element.ALIGN_CENTER));

				// Ventas últimos 30 días → derecha
				tabla.AddCell(Celda(item.vta_u30.ToString("N2"), chico, Element.ALIGN_RIGHT));

				// Nivel de servicio → derecha
				tabla.AddCell(Celda(item.stk > 0 ? "100%" : "0%", chico, Element.ALIGN_RIGHT));
			}


			pdf.Add(tabla);
		}


		private static void TablaPorRubro(Document pdf, List<ReporteEvalDeNivelDeServicioDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 5, 50, 10, 10, 10, 10, 10, 10 });

			AgregarCeldaHeader(tabla, "ID", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos AR", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock AR", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio AR", normalBold);

			tabla.HeaderRows = 1;

			foreach (var item in registros)
			{
				// ID del rubro → centrado
				tabla.AddCell(Celda(item.rub_id, chico, Element.ALIGN_CENTER));

				// Descripción → izquierda
				tabla.AddCell(Celda(item.rub_desc, chico, Element.ALIGN_LEFT));

				// Prod. Activos → derecha
				tabla.AddCell(Celda(item.cantidad.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock → derecha
				tabla.AddCell(Celda(item.cantidad_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio → derecha
				tabla.AddCell(Celda(item.ns.ToString("N2"), chico, Element.ALIGN_RIGHT));

				// Prod. Activos AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio AR → derecha
				tabla.AddCell(Celda(item.ns_ar.ToString("N2"), chico, Element.ALIGN_RIGHT));
			}


			pdf.Add(tabla);
		}


		private static void TablaPorSector(Document pdf, List<ReporteEvalDeNivelDeServicioDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 5, 50, 10, 10, 10, 10, 10, 10 });

			AgregarCeldaHeader(tabla, "ID", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos AR", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock AR", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio AR", normalBold);

			tabla.HeaderRows = 1;

			foreach (var item in registros)
			{
				// ID del rubro → centrado
				tabla.AddCell(Celda(item.sec_id, chico, Element.ALIGN_CENTER));

				// Descripción → izquierda
				tabla.AddCell(Celda(item.sec_desc, chico, Element.ALIGN_LEFT));

				// Prod. Activos → derecha
				tabla.AddCell(Celda(item.cantidad.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock → derecha
				tabla.AddCell(Celda(item.cantidad_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio → derecha
				tabla.AddCell(Celda(item.ns.ToString("N2"), chico, Element.ALIGN_RIGHT));

				// Prod. Activos AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio AR → derecha
				tabla.AddCell(Celda(item.ns_ar.ToString("N2"), chico, Element.ALIGN_RIGHT));
			}

			pdf.Add(tabla);
		}


		private static void TablaPorProveedor(Document pdf, List<ReporteEvalDeNivelDeServicioDto> registros, Font chico, Font normal, Font normalBold, Font chicoBold)
		{
			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 10, 50, 9, 9, 9, 9, 9, 9 });

			AgregarCeldaHeader(tabla, "ID", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio", normalBold);
			AgregarCeldaHeader(tabla, "Prod. Activos AR", normalBold);
			AgregarCeldaHeader(tabla, "Con Stock AR", normalBold);
			AgregarCeldaHeader(tabla, "Nivel de Servicio AR", normalBold);

			tabla.HeaderRows = 1;

			foreach (var item in registros)
			{
				// ID del rubro → centrado
				tabla.AddCell(Celda(item.cta_id, chico, Element.ALIGN_CENTER));

				// Descripción → izquierda
				tabla.AddCell(Celda(item.cta_denominacion, chico, Element.ALIGN_LEFT));

				// Prod. Activos → derecha
				tabla.AddCell(Celda(item.cantidad.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock → derecha
				tabla.AddCell(Celda(item.cantidad_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio → derecha
				tabla.AddCell(Celda(item.ns.ToString("N2"), chico, Element.ALIGN_RIGHT));

				// Prod. Activos AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Con Stock AR → derecha
				tabla.AddCell(Celda(item.cantidad_ar_stk.ToString("N0"), chico, Element.ALIGN_RIGHT));

				// Nivel de Servicio AR → derecha
				tabla.AddCell(Celda(item.ns_ar.ToString("N2"), chico, Element.ALIGN_RIGHT));
			}

			pdf.Add(tabla);
		}


		// ============================================================
		// HELPERS
		// ============================================================
		private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.BackgroundColor = new BaseColor(184, 134, 11); // Golden
			celda.Phrase.Font.Color = BaseColor.Black;
			tabla.AddCell(celda);
		}

		private static PdfPCell Celda(string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Padding = 3f;
			return c;
		}

		#endregion

		private List<ReporteEvalDeNivelDeServicioDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string tipoReporte, out string filtrosString)
		{
			try
			{
				var ret = new List<ReporteEvalDeNivelDeServicioDto>();
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
				ret = _consSrv.RepoEvalDeNivelDeServicio(new ReporteEvalDeNivelDeServicioRequest()
				{
					agrupador = Convert.ToInt32(agrupador),
					lFam = lFam,
					lProv=lProv,
					lRub=lRub,
					lSuc=lSuc
				});
				titulo = $"Reporte Evaluación de Nivel de Servicio {tipoReporte}";
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
			List<ReporteEvalDeNivelDeServicioDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
			List<ReporteEvalDeNivelDeServicioDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out tipoReporte, out filtrosString);

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
