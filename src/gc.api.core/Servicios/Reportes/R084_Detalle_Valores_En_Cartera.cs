using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
	public class R084_Detalle_Valores_En_Cartera : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IFinancieroServicio _finSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R084_Detalle_Valores_En_Cartera(IUnitOfWork uow, IFinancieroServicio finSrv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_finSrv = finSrv;
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
				List<FinancieroCarteraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoDetalleValoresEnCartera(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarRepoDetalleValoresEnCartera(Document pdf, List<FinancieroCarteraDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			var grupos = registros
				.GroupBy(r => r.fc_fecha_valor?.Date)
				.OrderBy(g => g.Key);

			foreach (var grupo in grupos)
			{
				DateTime? fechaVto = grupo.Key;
				string vtoTexto = fechaVto?.ToString("dd/MM/yyyy") ?? "";

				decimal totalGrupo = grupo.Sum(x => x.fc_importe);
				string totalGrupoTexto = totalGrupo.ToString("#,##0.00", new CultureInfo("es-AR"));

				PdfPTable tbl = new PdfPTable(8);
				tbl.WidthPercentage = 100;
				tbl.SetWidths(new float[] { 10f, 12f, 12f, 20f, 12f, 10f, 24f, 12f });

				// ENCABEZADOS
				AgregarHeader(tbl, "Vto", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Tot. Vto.", normalBold, Element.ALIGN_RIGHT);
				AgregarHeader(tbl, "Fecha Sis.", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Banco", normalBold, Element.ALIGN_LEFT);
				AgregarHeader(tbl, "N° Cheque", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Plaza", normalBold, Element.ALIGN_CENTER);
				AgregarHeader(tbl, "Cliente", normalBold, Element.ALIGN_LEFT);
				AgregarHeader(tbl, "Importe", normalBold, Element.ALIGN_RIGHT);

				bool primeraFila = true;

				foreach (var r in grupo)
				{
					int borde = primeraFila ? Rectangle.NO_BORDER : Rectangle.NO_BORDER;

					// Vto
					AgregarCelda(tbl, primeraFila ? vtoTexto : "", normal, Element.ALIGN_CENTER, borde);

					// Tot. Vto
					AgregarCelda(tbl, primeraFila ? totalGrupoTexto : "", normalBold, Element.ALIGN_RIGHT, borde);

					// Fecha Sis
					AgregarCelda(tbl, r.fc_fecha?.ToString("dd/MM/yyyy") ?? "", normal, Element.ALIGN_CENTER, borde);

					// Banco
					AgregarCelda(tbl, r.fc_dato1_valor ?? "", normal, Element.ALIGN_LEFT, borde);

					// N° Cheque
					AgregarCelda(tbl, r.fc_dato2_valor ?? "", normal, Element.ALIGN_CENTER, borde);

					// Plaza
					AgregarCelda(tbl, r.fc_dato3_valor ?? "", normal, Element.ALIGN_CENTER, borde);

					// Cliente
					string cliente = $"({r.cta_id}) {r.cta_denominacion}";
					AgregarCelda(tbl, cliente, normal, Element.ALIGN_LEFT, borde);

					// Importe
					AgregarCelda(tbl,
						r.fc_importe.ToString("#,##0.00", new CultureInfo("es-AR")),
						normal, Element.ALIGN_RIGHT, borde);

					primeraFila = false;
				}

				// 🔥 SEPARADOR FINAL DEL GRUPO (línea gruesa transversal)
				PdfPCell separador = new PdfPCell(new Phrase(""))
				{
					Border = Rectangle.BOTTOM_BORDER,
					BorderWidthBottom = 2f,
					Colspan = 8,
					PaddingTop = 4f,
					PaddingBottom = 4f
				};
				tbl.AddCell(separador);

				pdf.Add(tbl);

				// Espacio entre grupos
				pdf.Add(new Paragraph(" ", chico));
			}
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

		private static void AgregarCelda(PdfPTable tbl, string texto, Font font, int align, int border)
		{
			PdfPCell cel = new PdfPCell(new Phrase(texto, font));
			cel.HorizontalAlignment = align;
			cel.VerticalAlignment = Element.ALIGN_MIDDLE;
			cel.Border = border; // permite NO_BORDER en filas internas
			tbl.AddCell(cel);
		}

		private List<FinancieroCarteraDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var ret = new List<FinancieroCarteraDto>();
				var ctaf_id = solicitud.Parametros.GetValueOrDefault("ctaf_id", "")?.ToString() ?? null;
				var cta_id = "%";
				var ctaf_desc = solicitud.Parametros.GetValueOrDefault("ctaf_desc", "")?.ToString() ?? null;
				titulo = $"Detalle de Valores en Cartera";
				ret = _finSrv.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, cta_id);
				subtit = $"Cuenta: {ctaf_desc}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<FinancieroCarteraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<FinancieroCarteraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
