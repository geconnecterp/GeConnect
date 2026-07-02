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
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
	public class R086_Saldo_Cta_Distr_Detalle : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R086_Saldo_Cta_Distr_Detalle(IUnitOfWork uow, IConsultaServicio consSrv,
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
				List<SaldoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoSaldoDetalleCtaDistribuidora(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarRepoSaldoDetalleCtaDistribuidora(Document pdf, List<SaldoDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Agrupación por vendedor
			var gruposVendedor = registros
				.GroupBy(x => new { x.ve_id, x.ve_nombre })
				.OrderBy(g => g.Key.ve_id);

			foreach (var grupoV in gruposVendedor)
			{
				decimal totalVendedor = grupoV.Sum(x => x.cv_importe);

				// ================================
				// ENCABEZADO DEL VENDEDOR
				// ================================
				PdfPTable tblVendedor = new PdfPTable(1);
				tblVendedor.WidthPercentage = 100;

				// Construcción del texto:
				// Vendedor:(23) MINGUES, MAXIMILIANO 292,238.47
				Chunk lblVend = new Chunk("Vendedor:", normal);
				lblVend.SetUnderline(0.5f, -2f);

				Chunk lblId = new Chunk($"({grupoV.Key.ve_id}) ", normalBold);
				Chunk lblNom = new Chunk($"{grupoV.Key.ve_nombre} ", normalBold);
				Chunk lblTot = new Chunk($"{totalVendedor:N2}", normalBold);

				Phrase phrVend = new Phrase();
				phrVend.Add(lblVend);
				phrVend.Add(lblId);
				phrVend.Add(lblNom);
				phrVend.Add(lblTot);

				PdfPCell celVend = new PdfPCell(phrVend);
				celVend.Border = Rectangle.NO_BORDER;
				celVend.PaddingBottom = 6;
				celVend.BackgroundColor = new BaseColor(220, 230, 255); // azul suave
				tblVendedor.AddCell(celVend);

				pdf.Add(tblVendedor);

				// ================================
				// AGRUPACIÓN POR CLIENTE
				// ================================
				var gruposCliente = grupoV
					.GroupBy(x => new { x.cta_id, x.cta_denominacion })
					.OrderBy(g => g.Key.cta_id);

				foreach (var grupoC in gruposCliente)
				{
					decimal subtotalCliente = grupoC.Sum(x => x.cv_importe);

					// -----------------------------------------
					// ETIQUETA DEL CLIENTE
					// -----------------------------------------
					PdfPTable tblCliente = new PdfPTable(1);
					tblCliente.WidthPercentage = 100;

					Chunk lblCtaId = new Chunk($"({grupoC.Key.cta_id}) ", normalBold);
					Chunk lblCtaNom = new Chunk($"{grupoC.Key.cta_denominacion}", normalBold);

					Phrase phrCliente = new Phrase();
					phrCliente.Add(lblCtaId);
					phrCliente.Add(lblCtaNom);

					PdfPCell celCliente = new PdfPCell(phrCliente);
					celCliente.Border = Rectangle.NO_BORDER;
					celCliente.PaddingTop = 4;
					celCliente.PaddingBottom = 4;
					tblCliente.AddCell(celCliente);

					pdf.Add(tblCliente);

					// -----------------------------------------
					// TABLA DETALLE POR CLIENTE
					// -----------------------------------------
					PdfPTable tbl = new PdfPTable(7);
					tbl.WidthPercentage = 100;
					tbl.SetWidths(new float[] { 30, 8, 8, 12, 10, 15, 17 });

					// Encabezados
					AddHeader(tbl, "Descripción", normalBold);
					AddHeader(tbl, "Est.", normalBold);
					AddHeader(tbl, "Cuota", normalBold);
					AddHeader(tbl, "Mov Cta. Cte.", normalBold);
					AddHeader(tbl, "Día Atr.", normalBold);
					AddHeader(tbl, "Vencimiento", normalBold);
					AddHeader(tbl, "Importe", normalBold);

					// Filas
					foreach (var r in grupoC.OrderBy(x => x.cv_fecha_vto))
					{
						// Descripción: tco_desc + "-" + cm_compte (izq) + ve_id (der)
						// ===============================
						// Celda Descripción con 2 columnas
						// ===============================

						// Tabla interna: 2 columnas (izq = descripción, der = ve_id)
						PdfPTable tblDesc = new PdfPTable(2);
						tblDesc.WidthPercentage = 100;
						tblDesc.SetWidths(new float[] { 85, 15 });   // proporción fina
						tblDesc.DefaultCell.Border = Rectangle.NO_BORDER;

						// Columna izquierda: descripción
						PdfPCell celDescIzq = new PdfPCell(new Phrase($"{r.tco_desc}-{r.cm_compte}", normal));
						celDescIzq.Border = Rectangle.NO_BORDER;
						celDescIzq.HorizontalAlignment = Element.ALIGN_LEFT;
						celDescIzq.Padding = 0;
						tblDesc.AddCell(celDescIzq);

						// Columna derecha: (ve_id)
						PdfPCell celDescDer = new PdfPCell(new Phrase($"({r.ve_id})", chico));
						celDescDer.Border = Rectangle.NO_BORDER;
						celDescDer.HorizontalAlignment = Element.ALIGN_RIGHT;
						celDescDer.Padding = 0;
						tblDesc.AddCell(celDescDer);

						// Celda final que contiene la tabla interna
						PdfPCell celDescFinal = new PdfPCell(tblDesc);
						celDescFinal.Border = Rectangle.NO_BORDER;
						celDescFinal.PaddingTop = 0;
						celDescFinal.PaddingBottom = 0;

						tbl.AddCell(celDescFinal);

						AddCell(tbl, r.cv_estado, normal, Element.ALIGN_CENTER);
						AddCell(tbl, r.cm_compte_cuota.ToString(), normal, Element.ALIGN_CENTER);
						AddCell(tbl, r.dia_movi, normal, Element.ALIGN_CENTER);
						AddCell(tbl, r.atraso.ToString(), normal, Element.ALIGN_RIGHT);
						AddCell(tbl, r.cv_fecha_vto.ToString("dd/MM/yy"), normal, Element.ALIGN_CENTER);
						AddCell(tbl, r.cv_importe.ToString("N2"), normal, Element.ALIGN_RIGHT);
					}

					pdf.Add(tbl);

					// -----------------------------------------
					// SUBTOTAL DEL CLIENTE
					// -----------------------------------------
					PdfPTable tblSub = new PdfPTable(1);
					tblSub.WidthPercentage = 100;

					Phrase phrSub = new Phrase();
					phrSub.Add(new Chunk($"Total de \"{grupoC.Key.cta_denominacion}\": {subtotalCliente:N2}", normalBold));

					PdfPCell celSub = new PdfPCell(phrSub);
					celSub.Border = Rectangle.NO_BORDER;
					celSub.HorizontalAlignment = Element.ALIGN_RIGHT;
					celSub.PaddingTop = 4;
					celSub.PaddingBottom = 4;

					tblSub.AddCell(celSub);
					pdf.Add(tblSub);

					// -----------------------------------------
					// SEPARADOR ENTRE CLIENTES
					// -----------------------------------------
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
		}

		#endregion
		// =====================================================
		// HELPERS
		// =====================================================
		private static void AddHeader(PdfPTable tbl, string texto, Font font)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.Border = Rectangle.NO_BORDER;
			c.PaddingBottom = 4;
			tbl.AddCell(c);
		}

		private static void AddCell(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, font));
			c.HorizontalAlignment = align;
			c.Border = Rectangle.NO_BORDER;
			tbl.AddCell(c);
		}


		private List<SaldoDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var ret = new List<SaldoDetalleDto>();
				var vend_list_temp = solicitud.Parametros.GetValueOrDefault("Vendedores", "")?.ToString() ?? null;
				List<string> lista = vend_list_temp.Split(',').ToList();
				titulo = $"Saldo de Clientes x Vendedor";
				ret = _consSrv.BuscarSaldoDetalleCtaDistribuidora(new BuscarSaldoDetalleRequest() { 
					ve_list = lista,
				});
				subtit = $"(Detalle de Comprobantes)";
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
			List<SaldoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<SaldoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
