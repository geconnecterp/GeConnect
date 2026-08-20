using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R096_Consulta_Movimiento_De_Stock : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R096_Consulta_Movimiento_De_Stock(IUnitOfWork uow, IConsultaServicio consSrv,
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
				string filtrosString;
				List<MovStkProductoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
				CargarRepoMovimientoDeStock(pdf, registros, filtrosString, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoMovimientoDeStock(Document pdf, List<MovStkProductoDto> registros, string filtrosString, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron registros.", normal));
				return;
			}

			// ============================
			// TABLA
			// ============================
			PdfPTable tabla = new PdfPTable(7);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 10f, 35f, 10f, 10f, 25f, 10f, 10f });
			tabla.HeaderRows = 1;
			// ============================
			// HEADER
			// ============================
			AgregarCeldaHeader(tabla, "Fecha", normalBold);
			AgregarCeldaHeader(tabla, "Concepto", normalBold);
			AgregarCeldaHeader(tabla, "Box", normalBold);
			AgregarCeldaHeader(tabla, "Id", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "E/S", normalBold);
			AgregarCeldaHeader(tabla, "Stock", normalBold);

			// ============================
			// CUERPO
			// ============================
			string fechaAnterior = null;
			string conceptoAnterior = null;
			bool alt = false;

			foreach (var item in registros)
			{
				// Alternancia Golden
				BaseColor bg = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
				alt = !alt;

				// NUEVA REGLA: repetir solo si ambos coinciden
				bool repetirAmbos =
					item.FechaFormateada == fechaAnterior &&
					item.sm_concepto == conceptoAnterior;

				// FECHA
				AgregarCeldaDato(
					tabla,
					repetirAmbos ? "" : item.FechaFormateada,
					chico,
					bg,
					Element.ALIGN_CENTER);

				// CONCEPTO
				AgregarCeldaDato(
					tabla,
					repetirAmbos ? "" : item.sm_concepto,
					chico,
					bg,
					Element.ALIGN_LEFT);

				// BOX
				AgregarCeldaDato(
					tabla,
					item.box_id,
					chico,
					bg,
					Element.ALIGN_CENTER);

				// ID
				AgregarCeldaDato(
					tabla,
					item.p_id,
					chico,
					bg,
					Element.ALIGN_CENTER);

				// DESCRIPCIÓN
				AgregarCeldaDato(
					tabla,
					item.sm_desc,
					chico,
					bg,
					Element.ALIGN_LEFT);

				// E/S
				AgregarCeldaDato(
					tabla,
					FormatearMonto(item.sm_es, item.PermiteDecimales),
					chico,
					bg,
					Element.ALIGN_RIGHT);

				// STOCK
				AgregarCeldaDato(
					tabla,
					FormatearMonto(item.sm_stk, item.PermiteDecimales),
					chico,
					bg,
					Element.ALIGN_RIGHT);

				// Actualizar para la próxima iteración
				fechaAnterior = item.FechaFormateada;
				conceptoAnterior = item.sm_concepto;
			}


			pdf.Add(tabla);
		}


		private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.BackgroundColor = new BaseColor(184, 134, 11); // Golden
			celda.Padding = 4;
			tabla.AddCell(celda);
		}

		private static void AgregarCeldaDato(
			PdfPTable tabla,
			string texto,
			Font font,
			BaseColor bg,
			int align)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = align;
			celda.BackgroundColor = bg;
			celda.Padding = 3;
			celda.BorderColor = new BaseColor(220, 220, 220);
			tabla.AddCell(celda);
		}

		private static string FormatearMonto(decimal valor, bool permiteDecimales)
		{
			return permiteDecimales
				? valor.ToString("N2")
				: valor.ToString("N0");
		}


		#endregion

		private List<MovStkProductoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string filtrosString)
		{
			try
			{
				var ret = new List<MovStkProductoDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				var lTipoMov = solicitud.Parametros.GetValueOrDefault("lTipoMov", "")?.ToString() ?? null;
				var lProv = solicitud.Parametros.GetValueOrDefault("lProv", "")?.ToString() ?? null;
				var lDep = solicitud.Parametros.GetValueOrDefault("lDep", "")?.ToString() ?? null;
				var lBox = solicitud.Parametros.GetValueOrDefault("lBox", "")?.ToString() ?? null;
				var pId = solicitud.Parametros.GetValueOrDefault("pId", "")?.ToString() ?? null;

				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;
				ret = _consSrv.ConsultarProductoMovStk(new BuscarMovStockProductosRequest()
				{
					lMovTipo = [.. lTipoMov.Split(',')],
					lProv = [.. lProv.Split(',')],
					lDep = [.. lDep.Split(',')],
					lBox = [.. lBox.Split(',')],
					pId = pId,
					desde = desde,
					hasta = hasta,
					Registros = 999999999,
					Pagina = 1
				});
				titulo = $"Consulta de Movimiento de Stock";
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
			List<MovStkProductoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
			List<MovStkProductoDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
