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
	public class R089_Comisiones_Vendedores_Resumen : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R089_Comisiones_Vendedores_Resumen(IUnitOfWork uow, IConsultaServicio consSrv,
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
				List<ComisionesDeVendedoresResumenDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarComisionDeVenderesResumen(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarComisionDeVenderesResumen(Document pdf, List<ComisionesDeVendedoresResumenDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================
			PdfPTable tbl = new PdfPTable(3);
			tbl.WidthPercentage = 100;
			tbl.SetWidths(new float[] { 40, 30, 30 });

			// ============================================================
			// CABECERA (fondo gris + negrita + bordes visibles)
			// ============================================================
			AddHeader(tbl, "Vendedor", normalBold, Element.ALIGN_LEFT);
			AddHeader(tbl, "Vta. Neto", normalBold, Element.ALIGN_RIGHT);
			AddHeader(tbl, "Comi. Vta.", normalBold, Element.ALIGN_RIGHT);

			// ============================================================
			// FILAS
			// ============================================================
			decimal totalVtaNeta = 0;
			decimal totalComiVta = 0;

			foreach (var r in registros.OrderBy(x => x.ve_id))
			{
				string vendedor = $"({r.ve_id}) {r.ve_nombre}";
				decimal vtaNeta = r.comi_fac + r.comi_nc;
				decimal comiVta = r.comi_base_fac + r.comi_base_nc;

				totalVtaNeta += vtaNeta;
				totalComiVta += comiVta;

				AddCell(tbl, vendedor, normal, Element.ALIGN_LEFT);
				AddCell(tbl, vtaNeta.ToString("N2"), normal, Element.ALIGN_RIGHT);
				AddCell(tbl, comiVta.ToString("N2"), normal, Element.ALIGN_RIGHT);
			}

			pdf.Add(tbl);

			// ============================================================
			// FILA TOTAL FINAL (fondo gris + negrita + bordes visibles)
			// ============================================================
			PdfPTable tblTot = new PdfPTable(3);
			tblTot.WidthPercentage = 100;
			tblTot.SetWidths(new float[] { 40, 30, 30 });

			PdfPCell celTitulo = new PdfPCell(new Phrase("Total comisión", normalBold));
			celTitulo.HorizontalAlignment = Element.ALIGN_LEFT;
			celTitulo.BackgroundColor = new BaseColor(230, 230, 230);
			celTitulo.Border = Rectangle.BOX;
			celTitulo.BorderColor = BaseColor.Gray;
			celTitulo.BorderWidth = 0.8f;
			celTitulo.Padding = 4;

			PdfPCell celTotVta = new PdfPCell(new Phrase(totalVtaNeta.ToString("N2"), normalBold));
			celTotVta.HorizontalAlignment = Element.ALIGN_RIGHT;
			celTotVta.BackgroundColor = new BaseColor(230, 230, 230);
			celTotVta.Border = Rectangle.BOX;
			celTotVta.BorderColor = BaseColor.Gray;
			celTotVta.BorderWidth = 0.8f;
			celTotVta.Padding = 4;

			PdfPCell celTotComi = new PdfPCell(new Phrase(totalComiVta.ToString("N2"), normalBold));
			celTotComi.HorizontalAlignment = Element.ALIGN_RIGHT;
			celTotComi.BackgroundColor = new BaseColor(230, 230, 230);
			celTotComi.Border = Rectangle.BOX;
			celTotComi.BorderColor = BaseColor.Gray;
			celTotComi.BorderWidth = 0.8f;
			celTotComi.Padding = 4;

			tblTot.AddCell(celTitulo);
			tblTot.AddCell(celTotVta);
			tblTot.AddCell(celTotComi);

			pdf.Add(tblTot);
		}

		// ============================================================
		// HELPERS
		// ============================================================
		private static void AddHeader(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.Padding = 4;
			c.BackgroundColor = new BaseColor(230, 230, 230); // fondo gris
			c.Border = Rectangle.BOX;                        // bordes visibles
			c.BorderColor = BaseColor.Gray;
			c.BorderWidth = 0.8f;
			tbl.AddCell(c);
		}

		private static void AddCell(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.Border = Rectangle.BOX;
			c.BorderColor = BaseColor.Gray;
			c.BorderWidth = 0.5f;
			c.Padding = 3;
			tbl.AddCell(c);
		}


		#endregion
		
		private List<ComisionesDeVendedoresResumenDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			try
			{
				var ret = new List<ComisionesDeVendedoresResumenDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();

				ret = _consSrv.BuscarComisionDeVendedorResumen(new ComisionesDeVendedoresRequest()
				{
					Hasta = hasta,
					Desde = desde
				});
				titulo = $"Resumen de Comisiones por Vendedores";
				subtitulo = $"Desde: {desde.ToString("dd/MM/yyyy")} Hasta: {hasta.ToString("dd/MM/yyyy")}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtitulo = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<ComisionesDeVendedoresResumenDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<ComisionesDeVendedoresResumenDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
