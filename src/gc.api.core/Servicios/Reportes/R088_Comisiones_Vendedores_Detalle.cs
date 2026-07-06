using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R088_Comisiones_Vendedores_Detalle : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R088_Comisiones_Vendedores_Detalle(IUnitOfWork uow, IConsultaServicio consSrv,
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
				List<ComisionesDeVendedoresDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarComisionDeVenderesDetalle(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarComisionDeVenderesDetalle(Document pdf, List<ComisionesDeVendedoresDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Agrupación por vendedor
			var gruposVendedor = registros
				.GroupBy(x => new { x.ve_id, x.ve_nombre })
				.OrderBy(g => g.Key.ve_id);

			foreach (var grupoV in gruposVendedor)
			{
				var lista = grupoV.ToList();
				decimal totalVendedor = lista.Sum(x => x.cm_total);

				// ============================================================
				// ENCABEZADO DEL VENDEDOR (fondo gris + negrita)
				// ============================================================
				PdfPTable tblVend = new PdfPTable(2);
				tblVend.WidthPercentage = 100;
				tblVend.SetWidths(new float[] { 70, 30 });

				Phrase phrVend = new Phrase();
				phrVend.Add(new Chunk("Vendedor: ", normalBold));
				phrVend.Add(new Chunk($"({grupoV.Key.ve_id}) {grupoV.Key.ve_nombre}", normalBold));

				PdfPCell celVend1 = new PdfPCell(phrVend);
				celVend1.Border = Rectangle.NO_BORDER;
				celVend1.BackgroundColor = new BaseColor(230, 230, 230);
				celVend1.Padding = 5;
				celVend1.HorizontalAlignment = Element.ALIGN_LEFT;

				PdfPCell celVend2 = new PdfPCell(new Phrase($"Total: {totalVendedor:N2}", normalBold));
				celVend2.Border = Rectangle.NO_BORDER;
				celVend2.BackgroundColor = new BaseColor(230, 230, 230);
				celVend2.Padding = 5;
				celVend2.HorizontalAlignment = Element.ALIGN_RIGHT;

				tblVend.AddCell(celVend1);
				tblVend.AddCell(celVend2);

				pdf.Add(tblVend);

				// ============================================================
				// TABLA DETALLE
				// ============================================================
				PdfPTable tbl = new PdfPTable(8);
				tbl.WidthPercentage = 100;
				tbl.SetWidths(new float[] { 10, 35, 10, 10, 10, 10, 6, 12 });

				// Encabezados
				AddHeader(tbl, "Compte.", normalBold, Element.ALIGN_CENTER);
				AddHeader(tbl, "Cliente", normalBold, Element.ALIGN_LEFT);
				AddHeader(tbl, "Fecha", normalBold, Element.ALIGN_CENTER);
				AddHeader(tbl, "Pedido N°", normalBold, Element.ALIGN_CENTER);
				AddHeader(tbl, "Reparto N°", normalBold, Element.ALIGN_CENTER);
				AddHeader(tbl, "Vta. Neta", normalBold, Element.ALIGN_RIGHT);
				AddHeader(tbl, "% Comi.", normalBold, Element.ALIGN_RIGHT);
				AddHeader(tbl, "Comi. Vta.", normalBold, Element.ALIGN_RIGHT);

				// Filas
				foreach (var r in lista.OrderBy(x => x.pc_fecha))
				{
					AddCell(tbl, r.cm_compte, normal, Element.ALIGN_CENTER);

					AddCell(tbl, $"({r.cta_id}) {r.cta_denominacion}", normal, Element.ALIGN_LEFT);

					AddCell(tbl, r.pc_fecha.ToString("dd/MM/yy"), normal, Element.ALIGN_CENTER);

					AddCell(tbl, r.pc_compte, normal, Element.ALIGN_CENTER);

					AddCell(tbl, r.or_compte, normal, Element.ALIGN_CENTER);

					AddCell(tbl, r.ve_comi_base.ToString("N2"), normal, Element.ALIGN_RIGHT);

					AddCell(tbl, r.ve_comi_porc.ToString("N2"), normal, Element.ALIGN_RIGHT);

					AddCell(tbl, r.cm_total.ToString("N2"), normalBold, Element.ALIGN_RIGHT);
				}

				pdf.Add(tbl);

				// ============================================================
				// SEPARADOR ENTRE VENDEDORES
				// ============================================================
				PdfPTable tblSep = new PdfPTable(1);
				tblSep.WidthPercentage = 100;

				PdfPCell celSep = new PdfPCell(new Phrase(""));
				celSep.BorderWidthBottom = 1;
				celSep.BorderColorBottom = BaseColor.Gray;
				celSep.Border = Rectangle.BOTTOM_BORDER;
				celSep.PaddingBottom = 6;

				tblSep.AddCell(celSep);
				pdf.Add(tblSep);
			}
		}

		// ============================================================
		// HELPERS
		// ============================================================
		private static void AddHeader(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.PaddingBottom = 4;
			
			c.BackgroundColor = new BaseColor(230, 230, 230);
			c.Border = Rectangle.BOX;
			c.BorderWidth = 0.8f;
			c.BorderColor = BaseColor.Gray;
			tbl.AddCell(c);
		}

		private static void AddCell(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.Border = Rectangle.NO_BORDER;
			tbl.AddCell(c);
		}


		#endregion

		private List<ComisionesDeVendedoresDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			try
			{
				var ret = new List<ComisionesDeVendedoresDetalleDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				
				ret = _consSrv.BuscarComisionDeVendedorDetalle(new ComisionesDeVendedoresRequest()
				{
					Hasta = hasta,
					Desde = desde
				});
				titulo = $"Detalle de Ventas y Comisiones por Vendedores";
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
			List<ComisionesDeVendedoresDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<ComisionesDeVendedoresDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
