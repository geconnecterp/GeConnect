using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R041_DetalleLiquidacionDeHaberes : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IFinancieroServicio _financieroServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R041_DetalleLiquidacionDeHaberes(IUnitOfWork uow, IConsultaServicio consulta, IFinancieroServicio financieroServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;
			_financieroServicio = financieroServicio;

			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_cuentaSv = consultaSv;
			_logger = logger;
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
				List<LiqEmpleadoDetalleParaReporteDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				//writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

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
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();

				#region Generación de Cabecera               

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, logo, _empresaGeco);

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

				#region Lista de Flujo de Egresos
				HelperPdf.CargarLiquidacionDeHaberesDeEmpleados(pdf, registros, chico, normalBold);
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

		private List<LiqEmpleadoDetalleParaReporteDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo)
		{
			string le_compte = string.Empty;
			try
			{
				le_compte = solicitud.Parametros.GetValueOrDefault("id", "").ToString();
				var lista = _financieroServicio.GetLiqEmpDetalleParaReporte(le_compte);
				if (lista==null || lista.Count==0)
				{
					titulo = $"Liquidación para Haberes Nº {le_compte} \t Mes: ";
					subtitulo = "Error";
					return [];
				}
				var unItem = lista.FirstOrDefault();
				var periodo = unItem?.le_periodo;
				var anio = periodo!=null && periodo.Length>=6 ? periodo.Substring(0,4) : "0000";
				var mes = periodo != null && periodo.Length >= 6 ? periodo.Substring(4, 2) : "00";
				titulo = $"Liquidación para Haberes Nº {le_compte} \nMes: {mes} Año: {anio}";
				subtitulo = $"Aplicado el {unItem.le_fecha.ToString("dd/MM/yyyy")} - Concepto: {unItem.le_concepto}";
				return lista;	
			}
			catch (Exception)
			{
				titulo = $"Liquidación para Haberes Nº {le_compte} \t Mes: 08 Año: 2020";
				subtitulo = "Aplicado el 26/08/2021 - Concepto: la chocha";
				return [];
			}
			//return new List<LiqEmpleadoDetalleParaReporteDto>();
			///TODO MARCE: Completar una vez auw este el SP
			//string le_compte = solicitud.Parametros.GetValueOrDefault("id", "").ToString();
			//titulo = $"Liquidación para Haberes Nº {le_compte} \t Mes: 08 Año: 2020";
			//subtitulo = "Aplicado el 26/08/2021 - Concepto: la chocha";
			//var lista = _financieroServicio.GetLiqEmpDetalleParaReporte(le_compte);
			//var unItem = lista.FirstOrDefault();
			//var fechaAplicacion = unItem?.an_fecha.ToString("dd/MM/yyyy") ?? "";
			//var concepto = unItem?.an_concepto ?? "";
			//titulo = $"Empleados del Vale Anticipo N° {an_compte} \n Fecha de Aplicación: {fechaAplicacion}";
			//subtitulo = $"Concepto: {concepto}";
			//return lista;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subTit;
			List<LiqEmpleadoDetalleParaReporteDto> registros = ObtenerDatos(solicitud, out tit, out subTit);

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
			string subTit;
			List<LiqEmpleadoDetalleParaReporteDto> registros = ObtenerDatos(solicitud, out tit, out subTit);

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
