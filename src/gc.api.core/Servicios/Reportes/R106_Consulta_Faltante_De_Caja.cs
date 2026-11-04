using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas;
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
	public class R106_Consulta_Faltante_De_Caja : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiCajaServicio _iCajaSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R106_Consulta_Faltante_De_Caja(IUnitOfWork uow, IConsultaServicio consSrv,
											  IOptions<EmpresaGeco> empresa, IApiCajaServicio iCajaSrv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_iCajaSrv = iCajaSrv;
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
				List<FaltanteDeCajaListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
				CargarRepoFaltantesDeCaja(pdf, registros, filtrosString, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoFaltantesDeCaja(Document pdf, List<FaltanteDeCajaListaDto> registros, string filtrosString, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron registros.", normal));
				return;
			}

			registros = registros.OrderBy(x => x.usu_id).ThenBy(x => x.dia).ToList();

			string usuarioActual = string.Empty;

			PdfPTable tabla = null;

			decimal totalFaltante = 0;
			decimal totalSobrante = 0;
			decimal totalAcumulado = 0;

			Action agregarCabecera = () =>
			{
				tabla = new PdfPTable(8);
				tabla.WidthPercentage = 100;
				tabla.SpacingBefore = 3f;

				tabla.SetWidths(new float[]
				{
						12f, // Día
						28f, // Sucursal
						12f, // Parte
						10f, // Cierre
						12f, // Faltante
						12f, // Sobrante
						12f, // Acumulado
						10f  // Procesado
				});

				string[] titulos = { "Fecha", "Sucursal", "Parte", "Cierre", "Faltante", "Sobrante", "Acumulado", "Procesado"};

				foreach (var tituloCol in titulos)
				{
					PdfPCell c = new PdfPCell(new Phrase(tituloCol, chicoBold));
					c.HorizontalAlignment = Element.ALIGN_CENTER;
					c.VerticalAlignment = Element.ALIGN_MIDDLE;
					c.BackgroundColor = new BaseColor(230, 230, 230);
					c.Padding = 4;
					tabla.AddCell(c);
				}
			};

			Action agregarTotales = () =>
			{
				if (tabla == null)
					return;


				PdfPCell vacio = new PdfPCell(new Phrase(""));
				vacio.BorderWidth = 0;

				for (int i = 0; i < 4; i++)
					tabla.AddCell(vacio);

				PdfPCell faltante = new PdfPCell(
					new Phrase(totalFaltante.ToString("#,##0.00"), chicoBold));
				faltante.HorizontalAlignment = Element.ALIGN_RIGHT;
				faltante.BackgroundColor = new BaseColor(230, 230, 230);

				PdfPCell sobrante = new PdfPCell(
					new Phrase(totalSobrante.ToString("#,##0.00"), chicoBold));
				sobrante.HorizontalAlignment = Element.ALIGN_RIGHT;
				sobrante.BackgroundColor = new BaseColor(210, 210, 210);

				PdfPCell acumulado = new PdfPCell(
					new Phrase(totalAcumulado.ToString("#,##0.00"), chicoBold));
				acumulado.HorizontalAlignment = Element.ALIGN_RIGHT;
				acumulado.BackgroundColor = new BaseColor(230, 230, 230);

				PdfPCell procesado = new PdfPCell(new Phrase(""));
				procesado.BackgroundColor = new BaseColor(230, 230, 230);

				tabla.AddCell(faltante);
				tabla.AddCell(sobrante);
				tabla.AddCell(acumulado);
				tabla.AddCell(procesado);

				pdf.Add(tabla);
				pdf.Add(new Paragraph(" "));
			};

			foreach (var item in registros)
			{
				if (usuarioActual != item.usu_id)
				{
					if (!string.IsNullOrEmpty(usuarioActual))
					{
						agregarTotales();
					}

					usuarioActual = item.usu_id;

					totalFaltante = 0;
					totalSobrante = 0;
					totalAcumulado = 0;

					Paragraph usuario = new Paragraph(
						$"Cajero: {item.usu_apellidoynombre.ToUpper()}",
						normalBold);

					usuario.SpacingBefore = 8f;
					usuario.SpacingAfter = 3f;

					pdf.Add(usuario);

					agregarCabecera();
				}

				decimal acumulado = item.faltante - item.sobrante;

				totalFaltante += item.faltante;
				totalSobrante += item.sobrante;
				totalAcumulado += acumulado;

				PdfPCell celda;

				celda = new PdfPCell(
					new Phrase(item.dia.ToString("dd/MM/yyyy"), chico));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(item.adm_nombre ?? "", chico));
				celda.HorizontalAlignment = Element.ALIGN_LEFT;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(item.caja_nro_proceso.ToString(), chico));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(item.caja_nro_cierre.ToString(), chico));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(item.faltante.ToString("#,##0.00"), chico));
				celda.HorizontalAlignment = Element.ALIGN_RIGHT;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(item.sobrante.ToString("#,##0.00"), chico));
				celda.HorizontalAlignment = Element.ALIGN_RIGHT;
				tabla.AddCell(celda);

				celda = new PdfPCell(
					new Phrase(acumulado.ToString("#,##0.00"), chico));
				celda.HorizontalAlignment = Element.ALIGN_RIGHT;
				tabla.AddCell(celda);

				string check = item.procesado_bool == "SI"
					? "SI"
					: "NO";

				celda = new PdfPCell(
					new Phrase(check, chicoBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				tabla.AddCell(celda);
			}

			agregarTotales();
		}


		#endregion

		private List<FaltanteDeCajaListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string filtrosString)
		{
			try
			{
				var ret = new List<FaltanteDeCajaListaDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				var lSuc = solicitud.Parametros.GetValueOrDefault("lSuc", "")?.ToString() ?? null;

				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;
				ret = _iCajaSrv.FaltanteDeCajaConsulta(new FaltanteDeCajaListaRequest()
				{
					desde = desde,
					hasta = hasta,
					adm_list = lSuc	
				});
				titulo = $"Registro de Faltantes de Caja";
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
			List<FaltanteDeCajaListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
			List<FaltanteDeCajaListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
