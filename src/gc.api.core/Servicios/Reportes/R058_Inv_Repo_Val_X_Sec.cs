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
	public class R058_Inv_Repo_Val_X_Sec : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IInventarioServicio _inventarioServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R058_Inv_Repo_Val_X_Sec(IUnitOfWork uow, IInventarioServicio inventarioServicio,
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
				List<InvRepoValPorSecDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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

				#region Lista de Cheques Emitidos Propios
				CargarRepoInvValPorSec(pdf, registros, chico, normalBold);
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

		private List<InvRepoValPorSecDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			var inv_nro = solicitud.Parametros.GetValueOrDefault("inv_nro", "")?.ToString() ?? null;

			titulo = $"Inventario Valorizado por Sectores N° {inv_nro}";

			var lista = _inventarioServicio.GetReporteValorizacionPorSector(new ReporteInventarioRequest
			{
				inv_nro = inv_nro
			});
			subtitulo = $"Estado: {lista.First().inve_desc}";
			return lista;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<InvRepoValPorSecDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<InvRepoValPorSecDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoInvValPorSec(Document pdf, List<InvRepoValPorSecDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || lista.Count == 0)
			{
				Paragraph sinDatos = new Paragraph("No se encontraron datos", fuenteEtiqueta);
				sinDatos.Alignment = Element.ALIGN_CENTER;
				pdf.Add(sinDatos);
				return;
			}

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);

			// Tabla con 9 columnas
			PdfPTable tabla = new PdfPTable(9);
			tabla.WidthPercentage = 100;

			// Anchos proporcionales
			tabla.SetWidths(new float[] { 15, 15, 15, 10, 10, 10, 10, 10, 10 });

			tabla.HeaderRows = 2;

			// ============================
			// CABECERA NIVEL 1
			// ============================
			PdfPCell c1 = new(new Phrase("Sectores", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c1);

			PdfPCell c2 = new(new Phrase("Prod. Rubro", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c2);

			PdfPCell c3 = new(new Phrase("Prod. con Conteo", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c3);

			PdfPCell c4 = new(new Phrase("Cantidades", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c4);

			PdfPCell c5 = new(new Phrase("Valorización", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c5);

			// ============================
			// CABECERA NIVEL 2
			// ============================
			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			// ============================
			// FILAS DE DATOS
			// ============================
			decimal totalProdRubro = 0;
			decimal totalProdConConteo = 0;
			decimal totalStkCant = 0;
			decimal totalPlaniCant = 0;
			decimal totalStkVal = 0;
			decimal totalPlaniVal = 0;

			foreach (var item in lista)
			{
				decimal difCant = item.stk_cant - item.plani_cant;
				decimal difVal = item.stk_val - item.plani_val;

				totalProdRubro += item.prod_sec;
				totalProdConConteo += item.prod_sec_cont;
				totalStkCant += item.stk_cant;
				totalPlaniCant += item.plani_cant;
				totalStkVal += item.stk_val;
				totalPlaniVal += item.plani_val;

				tabla.AddCell(new PdfPCell(new Phrase(item.sec_desc, fuenteEtiqueta)));
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_sec.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_sec_cont.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			// ============================
			// FILA DE TOTALES
			// ============================
			decimal totalDifCant = totalStkCant - totalPlaniCant;
			decimal totalDifVal = totalStkVal - totalPlaniVal;

			PdfPCell totalCell = new PdfPCell(new Phrase("TOTAL", fuenteValor));
			totalCell.Colspan = 1;
			totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
			totalCell.BackgroundColor = BaseColor.LightGray;
			tabla.AddCell(totalCell);

			tabla.AddCell(new PdfPCell(new Phrase(totalProdRubro.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalProdConConteo.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			// Agregar tabla al PDF
			pdf.Add(tabla);

		}
		#endregion
	}
}
