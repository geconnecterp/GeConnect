using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R062_Pedido_De_Cliente : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiPedidoServicio _apiPedidoServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R062_Pedido_De_Cliente(IUnitOfWork uow, IApiPedidoServicio apiPedidoServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiPedidoServicio = apiPedidoServicio;
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
				List<PedidoProductoDto> registros = ObtenerDatos(solicitud, out tit);

				solicitud.Titulo = tit;

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
				CargarRepoPedidoDeCliente(pdf, registros, chico, normalBold);
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

		private List<PedidoProductoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			var pc_compte = solicitud.Parametros.GetValueOrDefault("pc_compte", "")?.ToString() ?? null;

			titulo = $"Pedido de Cliente N° {pc_compte}";

			return _apiPedidoServicio.ObtenerDetalleDePedido(pc_compte);
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<PedidoProductoDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<PedidoProductoDto> registros = ObtenerDatos(solicitud, out tit);

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
		public static void CargarRepoPedidoDeCliente(Document pdf, List<PedidoProductoDto> registros, Font chico, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;
			var cab = registros.First();

			// ============================
			// CABECERA DEL PEDIDO (2x2)
			// ============================

			PdfPTable cabecera = new PdfPTable(new float[] { 15f, 35f, 15f, 35f });
			cabecera.WidthPercentage = 100;
			cabecera.SpacingAfter = 5f;

			PdfPCell Celda(string texto, Font font, bool bold = false)
			{
				return new PdfPCell(new Phrase(texto, bold ? normalBold : font))
				{
					Border = Rectangle.NO_BORDER,
					Padding = 2f
				};
			}

			// Fila 1
			cabecera.AddCell(Celda("Cliente:", normalBold, true));
			cabecera.AddCell(Celda($"({cab.cta_id}) {cab.cta_denominacion}", chico));
			cabecera.AddCell(Celda("Fecha:", normalBold, true));
			cabecera.AddCell(Celda(cab.pc_fecha.ToString("dd/MM/yy"), chico));

			// Fila 2
			cabecera.AddCell(Celda("Vendedor:", normalBold, true));
			cabecera.AddCell(Celda(cab.ve_nombre, chico));
			cabecera.AddCell(Celda("Repartidor:", normalBold, true));
			cabecera.AddCell(Celda(cab.rp_nombre, chico));

			// Fila 3
			cabecera.AddCell(Celda("Estado:", normalBold, true));
			cabecera.AddCell(Celda(cab.pce_desc, chico));
			cabecera.AddCell(Celda("Reparto N°:", normalBold, true));
			cabecera.AddCell(Celda(cab.cm_compte, chico));

			// Fila 4
			cabecera.AddCell(Celda("Factura:", normalBold, true));
			cabecera.AddCell(Celda(cab.facturado, chico));
			cabecera.AddCell(Celda("Obs.:", normalBold, true));
			cabecera.AddCell(Celda(cab.pc_obs, chico));

			pdf.Add(cabecera);

			// ============================
			// SEPARADOR
			// ============================
			PdfPTable separador = new PdfPTable(1);
			separador.WidthPercentage = 100;

			PdfPCell linea = new PdfPCell()
			{
				BorderWidthBottom = 1f,
				BorderWidthTop = 0,
				BorderWidthLeft = 0,
				BorderWidthRight = 0,
				Padding = 2f
			};

			separador.AddCell(linea);
			pdf.Add(separador);


			// ============================
			// DETALLE POR RUBROS
			// ============================
			var grupos = registros
				.GroupBy(x => new { x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.rub_id);

			decimal totalPedido = 0m;
			decimal totalEntregado = 0m;

			foreach (var grupo in grupos)
			{
				// Título del Rubro
				Paragraph titulo = new(
					$"Rubros: {grupo.Key.rub_desc}",
					normalBold
				);
				titulo.SpacingBefore = 10f;
				titulo.SpacingAfter = 5f;
				pdf.Add(titulo);

				// ============================
				// TABLA IZQUIERDA (Pedido)
				// ============================
				PdfPTable tablaPedido = new(new float[] { 12f, 48f, 10f, 15f, 15f }); // 5 columnas
				tablaPedido.WidthPercentage = 100;
				tablaPedido.SplitLate = false;
				tablaPedido.SplitRows = false;
				tablaPedido.KeepTogether = true;

				AgregarCeldaHeader(tablaPedido, "Código", chico);
				AgregarCeldaHeader(tablaPedido, "Descripción", chico);
				AgregarCeldaHeader(tablaPedido, "Cant. Ped.", chico);
				AgregarCeldaHeader(tablaPedido, "Precio Vta.", chico);
				AgregarCeldaHeader(tablaPedido, "Total", chico);

				foreach (var item in grupo)
				{
					decimal totalLinea = item.pcd_pedida * item.pcd_pvta;
					totalPedido += totalLinea;

					tablaPedido.AddCell(new PdfPCell(new Phrase(item.p_id, chico)));
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.p_desc, chico)));
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.pcd_pedida.ToString("0.##"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.pcd_pvta.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaPedido.AddCell(new PdfPCell(new Phrase(totalLinea.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				// ============================
				// TABLA DERECHA (Entregado)
				// ============================
				PdfPTable tablaEntregado = new([60f, 40f]);
				tablaEntregado.WidthPercentage = 100;
				tablaEntregado.SplitLate = false;
				tablaEntregado.SplitRows = false;
				tablaEntregado.KeepTogether = true;

				AgregarCeldaHeader(tablaEntregado, "Cant. Ent.", chico);
				AgregarCeldaHeader(tablaEntregado, "Total", chico);

				foreach (var item in grupo)
				{
					decimal totalEnt = item.pcd_enviada * item.pcd_pvta;
					totalEntregado += totalEnt;

					tablaEntregado.AddCell(new PdfPCell(new Phrase(item.pcd_enviada.ToString("0.##"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaEntregado.AddCell(new PdfPCell(new Phrase(totalEnt.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				// ============================
				// CONTENEDOR (lado a lado)
				// ============================
				PdfPTable contenedor = new PdfPTable(new float[] { 70f, 30f });
				contenedor.WidthPercentage = 100;
				contenedor.SpacingBefore = 5f;
				contenedor.KeepTogether = true;

				// Celda izquierda → tablaPedido
				PdfPCell celdaPedido = new(tablaPedido)
				{
					Border = Rectangle.NO_BORDER,
					Padding = 0
				};
				contenedor.AddCell(celdaPedido);

				// Celda derecha → tablaEntregado
				PdfPCell celdaEntregado = new(tablaEntregado)
				{
					Border = Rectangle.NO_BORDER,
					PaddingLeft = 10f   // separación visual
				};
				contenedor.AddCell(celdaEntregado);

				pdf.Add(contenedor);

			}

			// ============================
			// TOTALES FINALES
			// ============================
			PdfPTable tablaTotales = new PdfPTable(new float[] { 70f, 30f });
			tablaTotales.WidthPercentage = 100;
			tablaTotales.SpacingBefore = 10f;

			// Celda izquierda → Total Pedido
			PdfPCell celdaTotalPedido = new PdfPCell(new Phrase($"Total: {totalPedido:0.00}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingRight = 10f
			};
			tablaTotales.AddCell(celdaTotalPedido);

			// Celda derecha → Total Entregado
			PdfPCell celdaTotalEntregado = new PdfPCell(new Phrase($"Total: {totalEntregado:0.00}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT
			};
			tablaTotales.AddCell(celdaTotalEntregado);

			pdf.Add(tablaTotales);
		}

		private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.BackgroundColor = new BaseColor(230, 230, 230);
			celda.Padding = 4f;
			tabla.AddCell(celda);
		}
		#endregion
	}
}
