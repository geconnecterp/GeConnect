using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R044_ConsultaVencPorTipoCtaTipoCompte : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IFinancieroServicio _financieroServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R044_ConsultaVencPorTipoCtaTipoCompte(IUnitOfWork uow, IConsultaServicio consulta, IFinancieroServicio financieroServicio,
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
				List<VencimientoListaDto> registros = ObtenerDatos(solicitud, out tit);

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

				#region Lista de Cheques Emitidos Propios
				///TODO MARCE: Seguir aca, generar el reporte, verificar si los datos estan llegando bien en el metodo ObtenerDatos
				//HelperPdf.CargarTablaChequesEmitidosPropios(pdf, registros, chico, normalBold);
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

		private static string GetTipoTexto(string tipo)
		{
			return tipo switch
			{
				"V" => "Vencidos",
				"E" => "Emitidos",
				_ => "Desconocido"
			};
		}

		private static string GetCadenaDeEstadosSeleccionados(ReporteSolicitudDto solicitud)
		{
			var aux = string.Empty;
			var listaTempo = new List<string>();
			var usu = GetBoolParam(solicitud.Parametros, "id_u_bool");
			var prov = GetBoolParam(solicitud.Parametros, "id_c_bool");
			var est = GetBoolParam(solicitud.Parametros, "id_e_bool");
			var fin = GetBoolParam(solicitud.Parametros, "id_f_bool");
			if (usu) listaTempo.Add("Usuarios");
			if (est) listaTempo.Add("Estados");
			if (fin) listaTempo.Add("Cuenta Banco");
			if (prov) listaTempo.Add("Proveedores");
			aux = string.Join(",", listaTempo);
			if (aux.Length > 0) aux = "Filtrado por " + aux;
			return aux;
		}

		private List<VencimientoListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			var fv = GetBoolParam(solicitud.Parametros, "fv");
			var fvDesde = solicitud.Parametros.GetValueOrDefault("fvDesde", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fvHasta = solicitud.Parametros.GetValueOrDefault("fvHasta", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fvDesdePrint = solicitud.Parametros.GetValueOrDefault("fvDesdePrint", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fvHastaPrint = solicitud.Parametros.GetValueOrDefault("fvHastaPrint", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");

			var fg = GetBoolParam(solicitud.Parametros, "fg");
			var fgDesde = solicitud.Parametros.GetValueOrDefault("fgDesde", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fgHasta = solicitud.Parametros.GetValueOrDefault("fgHasta", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fgDesdePrint = solicitud.Parametros.GetValueOrDefault("fgDesdePrint", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fgHastaPrint = solicitud.Parametros.GetValueOrDefault("fgHastaPrint", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");

			var id_ctc = GetBoolParam(solicitud.Parametros, "id_ctc");
			var ctc_list = solicitud.Parametros.GetValueOrDefault("ctc_list", "")?.ToString() ?? null;
			var id_ope = GetBoolParam(solicitud.Parametros, "id_ope");
			var ope_list = solicitud.Parametros.GetValueOrDefault("ope_list", "")?.ToString() ?? null;
			var id_tco = GetBoolParam(solicitud.Parametros, "id_tco");
			var tco_list = solicitud.Parametros.GetValueOrDefault("tco_list", "")?.ToString() ?? null;

			titulo = $"Informe de movimientos por Tipo de Comprobante";

			return _consultaServicio.ConsultarVencimientosPorTipo(new ConsultarVencimientosRequest()
			{
				fv = fv,
				fvDesde = Convert.ToDateTime(fvDesde),
				fvhasta = Convert.ToDateTime(fvHasta),
				fg = fg,
				fgDesde = Convert.ToDateTime(fgDesde),
				fghasta = Convert.ToDateTime(fgHasta),
				id_ctc = id_ctc,
				id_ope = id_ope,
				id_tco = id_tco,
				ctc_list = ctc_list.Split(',').ToList() ?? [],
				ope_list = ope_list.Split(',').ToList() ?? [],
				tco_list = tco_list.Split(',').ToList() ?? [],
				Registros = 999999999
			});
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<VencimientoListaDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<VencimientoListaDto> registros = ObtenerDatos(solicitud, out tit);

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
