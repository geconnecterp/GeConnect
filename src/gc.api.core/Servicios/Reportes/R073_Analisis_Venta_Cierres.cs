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
	public class R073_Analisis_Venta_Cierres : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R073_Analisis_Venta_Cierres(IUnitOfWork uow, IApiVentasServicio ventasSv,
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
				List<AnaVtaMesDetalleCierreDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoAnalisisDeVentaCierre(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<AnaVtaMesDetalleCierreDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
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
				var listaTemp = _ventasSv.ObtenerAnaVtaMesDetalleCierreLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis de Venta por Cierres del Mes {mes.PadLeft(2, '0')}-{periodo}";
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
			List<AnaVtaMesDetalleCierreDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<AnaVtaMesDetalleCierreDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoAnalisisDeVentaCierre(Document pdf, List<AnaVtaMesDetalleCierreDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página apaisada
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta - Cierres", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle de cierres por sucursal", titulo))
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
			PdfPTable tabla = new PdfPTable(11)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				3.0f, // Sucursal
				1.4f, // Proceso
				1.4f, // Cierre
				2.0f, // Facturación
				2.0f, // Fact. Dif.
				2.0f, // Cta Cte
				2.0f, // Cobranza
				2.0f, // Cob. Dif.
				2.0f, // Devoluciones y NC
				2.0f, // Créditos Usados
				2.0f  // A Rendir
			});

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
			AddHeader("Proceso");
			AddHeader("Cierre");
			AddHeader("Facturación");
			AddHeader("Fact. Dif.");
			AddHeader("Cta. Cte.");
			AddHeader("Cobranza");
			AddHeader("Cob. Dif.");
			AddHeader("Devoluciones y NC");
			AddHeader("Créditos Usados");
			AddHeader("A Rendir");

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Sucursal
				tabla.AddCell(new PdfPCell(new Phrase(r.adm_nombre, normal))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				// Proceso
				tabla.AddCell(new PdfPCell(new Phrase(r.caja_nro_proceso.ToString(), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Cierre
				tabla.AddCell(new PdfPCell(new Phrase(r.caja_nro_cierre.ToString(), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Facturación Diferida
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_dif.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cta. Cte.
				tabla.AddCell(new PdfPCell(new Phrase(r.co_ctacte.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza Diferida
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza_dif.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Devoluciones y NC
				tabla.AddCell(new PdfPCell(new Phrase(r.co_nota_credito.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Créditos Usados
				tabla.AddCell(new PdfPCell(new Phrase(r.co_creditos_usados.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// A Rendir
				tabla.AddCell(new PdfPCell(new Phrase(r.a_rendir.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}
		#endregion
	}
}
