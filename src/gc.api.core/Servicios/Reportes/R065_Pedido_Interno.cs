using DocumentFormat.OpenXml.Vml;
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
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R065_Pedido_Interno : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R065_Pedido_Interno(IUnitOfWork uow, IApiProductoServicio apiProdSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiProdSv = apiProdSv;
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
				List<PIDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoPedidoInterno(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<PIDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var pi_compte = solicitud.Parametros.GetValueOrDefault("id", "")?.ToString() ?? null;

				var listaTemp = _apiProdSv.PIDetalle(pi_compte);
				var item = listaTemp.First();
				titulo = $"Pedido Interno de Productos N° {pi_compte}";
				subtit = $"De Sucursal: {item.adm_id_gen_nombre}\nPara Sucursal: {item.adm_id_des_nombre}";
				return listaTemp;
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
			List<PIDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<PIDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoPedidoInterno(Document pdf, List<PIDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var reg0 = registros.First();

			// ================================
			// ENCABEZADO MANUAL
			// ================================
			AgregarEncabezado(pdf, reg0, normal, normalBold);

			// ================================
			// TÍTULO
			// ================================
			Paragraph tit = new Paragraph("Detalle de Productos Solicitados", titulo);
			tit.Alignment = Element.ALIGN_CENTER;
			tit.SpacingAfter = 10f;
			pdf.Add(tit);

			// ================================
			// TABLA PRINCIPAL
			// ================================
			PdfPTable tabla = new PdfPTable(6);
			tabla.WidthPercentage = 100;
			//tabla.SetWidths(new float[] { 10f, 55f, 15f, 20f });
			tabla.SetWidths(new float[] { 7f, 55f, 10f, 10f, 9, 9 });

			// Encabezados
			AgregarCeldaHeader(tabla, "Código", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Ref. Prov.", normalBold);
			AgregarCeldaHeader(tabla, "Código de Barras", normalBold);
			AgregarCeldaHeader(tabla, "STK Salón Vta.", normalBold);
			AgregarCeldaHeader(tabla, "STK Otros Depo.", normalBold);

			// ================================
			// AGRUPADOR ÚNICO POR RUBRO
			// ================================
			string grupoActual = "";

			foreach (var item in registros
				.OrderBy(x => x.rub_id)
				.ThenBy(x => x.p_id))
			{
				// Detectar salto de página
				if (writerFitsNewPage(pdf, tabla))
				{
					pdf.NewPage();
					AgregarEncabezado(pdf, reg0, normal, normalBold);

					Paragraph titulo2 = new Paragraph("Detalle de Productos Solicitados", normalBold);
					titulo2.Alignment = Element.ALIGN_CENTER;
					titulo2.SpacingAfter = 10f;
					pdf.Add(titulo2);

					// Reimprimir encabezados
					AgregarCeldaHeader(tabla, "Código", normalBold);
					AgregarCeldaHeader(tabla, "Descripción", normalBold);
					AgregarCeldaHeader(tabla, "Ref. Prov.", normalBold);
					AgregarCeldaHeader(tabla, "Código de Barras", normalBold);
					AgregarCeldaHeader(tabla, "STK Salón Vta.", normalBold);
					AgregarCeldaHeader(tabla, "STK Otros Depo.", normalBold);
				}

				// ---- ÚNICO AGRUPADOR ----
				string grupo = $"{item.rub_desc} ({item.rub_id})";

				if (grupo != grupoActual)
				{
					PdfPCell celdaGrupo = new(new Phrase(grupo, normalBold))
					{
						Colspan = 6,
						BackgroundColor = new BaseColor(230, 230, 230),
						Padding = 5,
						HorizontalAlignment = Element.ALIGN_CENTER
					};
					tabla.AddCell(celdaGrupo);

					grupoActual = grupo;
				}

				// ---- Fila de producto ----
				AgregarCelda(tabla, item.p_id, chico, Element.ALIGN_CENTER);
				AgregarCelda(tabla, item.p_desc, chico, Element.ALIGN_LEFT);
				AgregarCelda(tabla, item.p_id_prov ?? "", chico, Element.ALIGN_CENTER);
				AgregarCelda(tabla, item.p_id_barrado, chico, Element.ALIGN_CENTER);
				AgregarCelda(tabla, GridHelper.FormatearDato(item.stk_dest_salon, GridHelper.FormatDato.Monto, item.PermiteDecimales), chico, Element.ALIGN_RIGHT);
				AgregarCelda(tabla, GridHelper.FormatearDato(item.stk_dest, GridHelper.FormatDato.Monto, item.PermiteDecimales), chico, Element.ALIGN_RIGHT);
			}

			pdf.Add(tabla);
		}

		private static void AgregarCelda(PdfPTable tabla, string texto, Font fuente, int Align = 0, bool esEncabezado = false, BaseColor? fondo = null)
		{
			var celda = new PdfPCell(new Phrase(texto, fuente))
			{
				HorizontalAlignment = Align,
				Padding = 4f
			};
			if (esEncabezado && fondo != null)
				celda.BackgroundColor = fondo;

			tabla.AddCell(celda);
		}

		private static void AgregarEncabezado(Document pdf, PIDetalleDto reg, Font normal, Font bold)
		{
			PdfPTable header = new(4)
			{
				WidthPercentage = 100
			};
			header.SetWidths([20f, 30f, 20f, 30f]);

			header.AddCell(new PdfPCell(new Phrase("Fecha Pedido:", bold))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_RIGHT
			});
			header.AddCell(new PdfPCell(new Phrase(reg.pi_fecha.ToString("dd/MM/yyyy"), normal))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_LEFT
			});

			header.AddCell(new PdfPCell(new Phrase("Solicitado Por:", bold))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_RIGHT
			});
			header.AddCell(new PdfPCell(new Phrase(reg.usu_apellidoynombre, normal))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_LEFT
			});

			header.SpacingAfter = 10f;

			pdf.Add(header);
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

		private static bool writerFitsNewPage(Document pdf, PdfPTable tabla)
		{
			return tabla.TotalHeight > (pdf.PageSize.Height - pdf.TopMargin - pdf.BottomMargin - 100);
		}
		#endregion
	}
}
