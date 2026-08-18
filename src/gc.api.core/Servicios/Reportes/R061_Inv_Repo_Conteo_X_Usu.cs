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
using static gc.infraestructura.Helpers.HelperPdf;

namespace gc.api.core.Servicios.Reportes
{
	public class R061_Inv_Repo_Conteo_X_Usu : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IInventarioServicio _inventarioServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R061_Inv_Repo_Conteo_X_Usu(IUnitOfWork uow, IInventarioServicio inventarioServicio,
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
				List<InvRepoConteosPorUsuDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoConteoPorUsu(pdf, registros, chico, normalBold);
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

		private List<InvRepoConteosPorUsuDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			var inv_nro = solicitud.Parametros.GetValueOrDefault("inv_nro", "")?.ToString() ?? null;
			var usu_id = solicitud.Parametros.GetValueOrDefault("usu_id", "")?.ToString() ?? null;
			var usu_nombre = solicitud.Parametros.GetValueOrDefault("usu_nombre", "")?.ToString() ?? null;

			titulo = $"Planilla de Carga de {usu_nombre}";
			subtitulo = $"Inventario N°: {inv_nro}";

			return _inventarioServicio.GetReporteConteosPorUsu(new ReporteInventarioRequest
			{
				inv_nro = inv_nro,
				usu_id = usu_id
			});
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<InvRepoConteosPorUsuDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<InvRepoConteosPorUsuDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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

		#region funcions
		public static void CargarRepoConteoPorUsu(Document pdf, List<InvRepoConteosPorUsuDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fuenteEtiqueta));
				return;
			}

			// Agrupar por planilla
			var grupos = lista
				.GroupBy(x => new { x.carga_nro, x.carga_des })
				.OrderBy(g => g.Key.carga_nro);

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);
			Font fuenteTitulo = new Font(fuenteEtiqueta.BaseFont, 14, Font.BOLD);
			bool primera = true;

			foreach (var grupo in grupos)
			{
				// Nueva hoja por cada planilla excepto la primera
				if (!primera)
					pdf.NewPage();
				primera = false;

				// ============================================================
				// TABLA COMPLETA (TÍTULO + CABECERA + DATOS)
				// ============================================================
				PdfPTable tabla = new PdfPTable(7);
				tabla.WidthPercentage = 100;
				tabla.SetWidths(new float[] { 8, 12, 35, 10, 10, 12, 13 });

				// Título + cabecera deben repetirse
				tabla.HeaderRows = 3;

				// ============================================================
				// FILA 1: TÍTULO (se repite en cada página)
				// ============================================================
				string titulo = $"{grupo.Key.carga_des} ({grupo.Key.carga_nro})";

				PdfPCell celdaTitulo = new PdfPCell(new Phrase(titulo, fuenteTitulo))
				{
					Colspan = 7,
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingBottom = 6f
				};

				// Subrayado
				celdaTitulo.CellEvent = new SubrayadoCellEvent();

				tabla.AddCell(celdaTitulo);

				// ============================================================
				// FILA 2 y 3: CABECERA REAL (igual al HTML)
				// ============================================================

				// CABECERA FILA 1
				PdfPCell c1 = new PdfPCell(new Phrase("Código", fuenteValor))
				{
					Rowspan = 2,
					Colspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c1);

				PdfPCell c2 = new PdfPCell(new Phrase("Producto", fuenteValor))
				{
					Rowspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c2);

				PdfPCell c3 = new PdfPCell(new Phrase("UP", fuenteValor))
				{
					Rowspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c3);

				PdfPCell c4 = new PdfPCell(new Phrase("Stock Inventariado", fuenteValor))
				{
					Colspan = 3,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
				tabla.AddCell(c4);

				// CABECERA FILA 2
				tabla.AddCell(new PdfPCell(new Phrase("Bulto", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase("Cant. Suelta", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase("Cant. Total", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });

				// ============================================================
				// FILAS DE DATOS
				// ============================================================
				foreach (var item in grupo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id_barrado, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_unidad_pres.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_bulto.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_unidad_suelta.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_cantidad.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				pdf.Add(tabla);
			}
		}
		#endregion
	}
}
