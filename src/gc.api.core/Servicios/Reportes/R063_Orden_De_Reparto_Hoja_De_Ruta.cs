using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R063_Orden_De_Reparto_Hoja_De_Ruta : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiOrdenDeRepartoServicio _apiOrdenReparto;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R063_Orden_De_Reparto_Hoja_De_Ruta(IUnitOfWork uow, IApiOrdenDeRepartoServicio apiOrdenReparto,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiOrdenReparto = apiOrdenReparto;
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
				List<PedidoEnOrdenDeRepartoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoHojaDeRutaDeOrdenDeReparto(pdf, registros, chico, normal, normalBold);
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

		private List<PedidoEnOrdenDeRepartoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var or_compte = solicitud.Parametros.GetValueOrDefault("orCompte", "")?.ToString() ?? null;

				titulo = $"Orden de Reparto N° {or_compte} - HOJA DE RUTA";

				var listaTemp = _apiOrdenReparto.ObtenerPedidosEnOrdenDeReparto(or_compte);
				var item = listaTemp.First();
				subtit = $"Repartidor: {item.rp_nombre} Fecha: {item.or_fecha:dd/MM/yyyy}\nObs: {item.or_obs}";
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
			List<PedidoEnOrdenDeRepartoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<PedidoEnOrdenDeRepartoDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoHojaDeRutaDeOrdenDeReparto(Document pdf, List<PedidoEnOrdenDeRepartoDto> registros, Font chico, Font normal, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================
			// ENCABEZADO GENERAL DE LA OR
			// ============================
			var or = registros.First();

			// ============================
			// AGRUPAR POR CLIENTE
			// ============================
			var grupos = registros
				.GroupBy(x => new
				{
					x.cta_id,
					x.cta_denominacion,
					x.cta_domicilio,
					x.cta_te,
					x.cta_celu
				})
				.OrderBy(g => g.Key.cta_denominacion);

			decimal totalGeneral = 0;

			foreach (var grupo in grupos)
			{
				// ============================================================
				// SEPARADOR ANTES DEL CLIENTE (ANCHO COMPLETO)
				// ============================================================
				AgregarSeparador(pdf, chico);

				// ============================================================
				// TABLA DE CLIENTE (4 CELDAS)
				// ============================================================
				PdfPTable tablaCliente = new PdfPTable(new float[] { 20, 30, 30, 20 });
				tablaCliente.WidthPercentage = 100;

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Cliente: ({grupo.Key.cta_id})", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					grupo.Key.cta_denominacion, normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Domi: {grupo.Key.cta_domicilio}", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				string telefono = $"{grupo.Key.cta_te}".Trim();
				if (!string.IsNullOrWhiteSpace(grupo.Key.cta_celu))
					telefono += $" / {grupo.Key.cta_celu}";

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Tel: {telefono}", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				pdf.Add(tablaCliente);

				pdf.Add(new Paragraph(" ", chico)); // pequeño espacio

				// ============================================================
				// TABLA DE PEDIDOS (3 CELDAS + SANGRÍA)
				// ============================================================
				PdfPTable tablaPedidos = new PdfPTable(new float[] { 25, 55, 20 });
				tablaPedidos.WidthPercentage = 100;

				foreach (var ped in grupo)
				{
					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"    Pedido N°: {ped.pc_compte}", normalBold))   // sangría con espacios
					{
						Border = Rectangle.NO_BORDER
					});

					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"Comprobante: {ped.tco_desc} {ped.cm_compte}", normalBold))
					{
						Border = Rectangle.NO_BORDER
					});

					var precio = ped.pc_precio_tot.ToString("N2");
					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"Importe: {precio}", normalBold))
					{
						Border = Rectangle.NO_BORDER,
						HorizontalAlignment = Element.ALIGN_RIGHT
					});

					totalGeneral += ped.pc_precio_tot;
				}

				pdf.Add(tablaPedidos);

				// ============================================================
				// SEPARADOR ENTRE GRUPOS (ANCHO COMPLETO)
				// ============================================================
				AgregarSeparador(pdf, chico);
			}

			// ============================
			// TOTAL GENERAL DE LA OR
			// ============================
			PdfPTable tablaTotal = new PdfPTable(1);
			tablaTotal.WidthPercentage = 100;

			tablaTotal.AddCell(new PdfPCell(new Phrase(
				$"TOTAL ORDEN DE REPARTO: {totalGeneral:N2}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 10
			});

			pdf.Add(tablaTotal);

		}

		private static void AgregarSeparador(Document pdf, Font chico)
		{
			PdfPTable sep = new PdfPTable(1);
			sep.WidthPercentage = 100;

			PdfPCell cellSep = new PdfPCell(new Phrase(" ", chico))
			{
				Border = Rectangle.BOTTOM_BORDER,
				BorderWidthBottom = 1f,
				PaddingTop = 4,
				PaddingBottom = 4
			};

			sep.AddCell(cellSep);
			pdf.Add(sep);
		}
		#endregion
	}
}
