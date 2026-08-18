using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.infraestructura.Dtos.Inventario.Request;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R057_Inv_Repo_Stk_Vs_Conteo : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IInventarioServicio _inventarioServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R057_Inv_Repo_Stk_Vs_Conteo(IUnitOfWork uow, IInventarioServicio inventarioServicio,
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
				List<InvRepoStkVsConteoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoStkVsConteo(pdf, registros, chico, normalBold, titulo);
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

		private List<InvRepoStkVsConteoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			var inv_nro = solicitud.Parametros.GetValueOrDefault("inv_nro", "")?.ToString() ?? null;

			titulo = $"Detalle de Inventario Registro de Stock vs. Conteo N°: {inv_nro}";

			var lista = _inventarioServicio.GetReporteStockVsConteo(new ReporteInventarioRequest
			{ 
				inv_nro = inv_nro,
			});
			subtitulo = lista.First().inve_desc;
			return lista;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<InvRepoStkVsConteoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<InvRepoStkVsConteoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoStkVsConteo(Document pdf, List<InvRepoStkVsConteoDto> lista, Font fChico, Font fNormal, Font fTitulo)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fNormal));
				return;
			}
			BaseColor amarilloPastel = new(255, 245, 200);
			// Tipo de inventario (se repite en todos los registros)
			var tipo = lista.First();
			bool incluyeGrupo2 = tipo.invt_id == 'D'; // si es 'D', agregar columna extra

			// ============================================================
			// TÍTULO DEL REPORTE
			// ============================================================
			//Paragraph titulo = new Paragraph($"Reporte: {tipo.invt_desc}", fTitulo);
			//titulo.SpacingAfter = 10f;
			//pdf.Add(titulo);

			// ============================================================
			// DEFINICIÓN DE COLUMNAS
			// ============================================================
			int columnas = incluyeGrupo2 ? 7 : 6;

			PdfPTable tabla = new(columnas)
			{
				WidthPercentage = 100
			};

			if (incluyeGrupo2)
			{
				tabla.SetWidths(new float[] { 10, 40, 10, 10, 10, 10, 10 });
			}
			else
			{
				tabla.SetWidths(new float[] { 10, 50, 10, 10, 10, 10 });
			}

			tabla.HeaderRows = 1;

			// ============================================================
			// CABECERA
			// ============================================================
			tabla.AddCell(CeldaHeader("Código", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Descripción", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Dif. a Ajustar", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Aplico Ajuste", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Conteo Grupo 1", fNormal, amarilloPastel));

			if (incluyeGrupo2)
				tabla.AddCell(CeldaHeader("Grupo Conteo 2", fNormal, amarilloPastel));

			// ============================================================
			// FILAS DE DATOS
			// ============================================================
			bool alt = true;

			foreach (var item in lista)
			{
				BaseColor fondo = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
				alt = !alt;

				decimal difAjustar = item.ps_conteo - item.ps_stk;

				tabla.AddCell(CeldaDato(item.p_id, fChico, fondo));
				tabla.AddCell(CeldaDato(item.p_des, fChico, fondo));
				tabla.AddCell(CeldaDato(difAjustar.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

				// Checkbox
				string chk = item.ps_ajuste == 'S' ? "✔" : "✘";
				tabla.AddCell(CeldaDato(chk, fChico, fondo, Element.ALIGN_CENTER));
				tabla.AddCell(CeldaDato(GridHelper.FormatearDato(item.ps_stk, GridHelper.FormatDato.Monto, item.PermiteDecimales), fChico, fondo, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaDato(item.conteo1.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

				if (incluyeGrupo2)
					tabla.AddCell(CeldaDato(item.conteo2.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

			}

			pdf.Add(tabla);
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

		private static PdfPCell CeldaDato(string texto, Font f, BaseColor fondo, int align = Element.ALIGN_LEFT)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = fondo;
			c.HorizontalAlignment = align;
			return c;
		}
		#endregion
	}
}
