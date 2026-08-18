using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static gc.infraestructura.Helpers.GridHelper;

namespace gc.api.core.Servicios.Reportes
{
	public class R077_Analisis_De_Valores_De_Venta_Pv : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R077_Analisis_De_Valores_De_Venta_Pv(IUnitOfWork uow, IApiVentasServicio ventasSv,
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
				List<AnaValDeVtaDetPVDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoAnalisisDeValoresDeVentaPV(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<AnaValDeVtaDetPVDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var sucursales = solicitud.Parametros.GetValueOrDefault("Sucursales", "")?.ToString() ?? null;
				var sucursalesTextos = solicitud.Parametros.GetValueOrDefault("SucursalesTextos", "")?.ToString() ?? null;
				var desde = solicitud.Parametros.GetValueOrDefault("Desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("Hasta", "").ToDateTime();
				var request = new AnaDeValDeVtaMesRequest()
				{
					adm_list = sucursales,
					desde = desde,
					hasta = hasta
				};
				var listaTemp = _ventasSv.ObtenerAnaDeValDeVtaDetPVLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis de Valores de Venta por PV desde el {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";
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

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<AnaValDeVtaDetPVDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<AnaValDeVtaDetPVDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoAnalisisDeValoresDeVentaPV(Document pdf, List<AnaValDeVtaDetPVDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Título
			Paragraph tituloPar = new("Análisis de Valores de Venta - Detalle por Punto de Venta", tituloBig)
			{
				Alignment = Element.ALIGN_CENTER,
				SpacingAfter = 10f
			};
			pdf.Add(tituloPar);

			// Definición de columnas
			float[] widths = { 1.6f, 2.5f, 1.4f, 2f, 2f, 2f, 2f, 2f, 2f };
			PdfPTable tabla = new(widths)
			{
				WidthPercentage = 100
			};

			// Encabezados
			string[] headers = {
				"Día",
				"Suc.",
				"PV",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutual",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(celda);
			}

			// Filas
			foreach (var item in registros)
			{
				// Día
				tabla.AddCell(new PdfPCell(new Phrase(item.dia.ToString("dd/MM/yyyy"), normal)));

				// Sucursal
				PdfPCell suc = new(new Phrase(item.adm_nombre, normal))
				{
					HorizontalAlignment = Element.ALIGN_LEFT
				};
				tabla.AddCell(suc);

				// PV
				PdfPCell pv = new(new Phrase(item.caja_id, normal))
				{
					HorizontalAlignment = Element.ALIGN_CENTER
				};
				tabla.AddCell(pv);

				// Efectivo
				tabla.AddCell(CeldaSoloMonto(item.efectivos, normal));

				// Tarjetas
				tabla.AddCell(CeldaSoloMonto(item.tarjetas, normal));

				// Transferencias
				tabla.AddCell(CeldaSoloMonto(item.bco_transf, normal));

				// Mutual
				tabla.AddCell(CeldaSoloMonto(item.mutuales, normal));

				// Vales
				tabla.AddCell(CeldaSoloMonto(item.vales, normal));

				// Otros
				tabla.AddCell(CeldaSoloMonto(item.otros, normal));
			}

			pdf.Add(tabla);
		}

		private static PdfPCell CeldaSoloMonto(decimal monto, Font normal)
		{
			PdfPCell c = new(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE, // ← clave
				Padding = 4
			};

			return c;
		}
		#endregion
	}
}
