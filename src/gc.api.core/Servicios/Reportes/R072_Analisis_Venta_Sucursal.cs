using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R072_Analisis_Venta_Sucursal : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R072_Analisis_Venta_Sucursal(IUnitOfWork uow, IApiVentasServicio ventasSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_ventasSv = ventasSv;
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
				List<AnaVtaMesDetalleSucursalDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoAnalisisDeVentaSucursal(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<AnaVtaMesDetalleSucursalDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var sucursales = solicitud.Parametros.GetValueOrDefault("Sucursales", "")?.ToString() ?? null;
				var sucursalesTextos = solicitud.Parametros.GetValueOrDefault("SucursalesTextos", "")?.ToString() ?? null;
				var mes = solicitud.Parametros.GetValueOrDefault("Mes", "")?.ToString() ?? null;
				var periodo = solicitud.Parametros.GetValueOrDefault("Periodo", "")?.ToString() ?? null;
				var (desde, hasta) = ObtenerRangoMes(Convert.ToInt32(periodo), Convert.ToInt32(mes));
				var request = new AnaVtaMesRequest()
				{
					adm_list = sucursales,
					desde = desde,
					hasta = hasta
				};
				var listaTemp = _ventasSv.ObtenerAnaVtaMesDetalleSucursalLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis de Venta por Sucursal del Mes {mes.PadLeft(2, '0')}-{periodo}";
				subtit = $"Sucursales: {sucursalesTextos}";
				return listaTemp;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				return [];
			}

		}

		public static (DateTime Desde, DateTime Hasta) ObtenerRangoMes(int periodo, int mes)
		{
			var desde = new DateTime(periodo, mes, 1);
			var hasta = desde.AddMonths(1).AddDays(-1);

			return (desde, hasta);
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<AnaVtaMesDetalleSucursalDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<AnaVtaMesDetalleSucursalDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoAnalisisDeVentaSucursal(Document pdf, List<AnaVtaMesDetalleSucursalDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página vertical (no apaisada)
			pdf.SetPageSize(PageSize.A4);
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta por Sucursal", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por sucursal", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(6)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				3.0f, // Sucursal
				2.0f, // Facturación
				1.6f, // Porcentaje
				2.0f, // Cta Cte
				2.0f, // Cobranza
				2.0f  // Rentabilidad
			});

			// ============================
			// COLORES PARA PORCENTAJES
			// ============================
			BaseColor ColorPorcentaje(decimal valor)
			{
				if (valor > 0) return new BaseColor(201, 228, 255); // celeste
				if (valor < 0) return new BaseColor(255, 224, 224); // rojo suave
				return BaseColor.White;
			}

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Sucursal");
			AddHeader("Facturación");
			AddHeader("Porcentaje");
			AddHeader("Cta. Cte.");
			AddHeader("Cobranza");
			AddHeader("Rentabilidad");

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Sucursal
				tabla.AddCell(new PdfPCell(new Phrase(r.adm_nombre, normal))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Porcentaje
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_porc.ToString("N2") + "%", normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = ColorPorcentaje(r.co_facturacion_porc), });

				// Cta. Cte.
				tabla.AddCell(new PdfPCell(new Phrase(r.co_ctacte.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}
		#endregion
	}
}
