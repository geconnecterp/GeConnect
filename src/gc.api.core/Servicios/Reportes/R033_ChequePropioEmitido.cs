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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R033_ChequePropioEmitido : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IFinancieroServicio _financieroServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R033_ChequePropioEmitido(IUnitOfWork uow, IConsultaServicio consulta, IFinancieroServicio financieroServicio,
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
				List<FinancieroBcoVencChequeEmitidoListaDto> registros = ObtenerDatos(solicitud, out tit);

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
				HelperPdf.CargarTablaChequesEmitidosPropios(pdf, registros, chico, normalBold);
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

		private List<FinancieroBcoVencChequeEmitidoListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			var fDesdePrint = solicitud.Parametros.GetValueOrDefault("desde1Print", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fHastaPrint = solicitud.Parametros.GetValueOrDefault("hasta2Print", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			
			var id_f = GetBoolParam(solicitud.Parametros, "id_f_bool");
			var ctaf_id = solicitud.Parametros.GetValueOrDefault("id_f", "")?.ToString() ?? null;
			var id_c = GetBoolParam(solicitud.Parametros, "id_c_bool");
			var cta_id = solicitud.Parametros.GetValueOrDefault("id_c", "")?.ToString() ?? null;
			var id_u = GetBoolParam(solicitud.Parametros, "id_u_bool");
			var usu_id = solicitud.Parametros.GetValueOrDefault("id_u", "")?.ToString() ?? null;
			var tipo_fecha = solicitud.Parametros.GetValueOrDefault("tipo_fecha", "").ToString();
			var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var estado = solicitud.Parametros.GetValueOrDefault("id_e", "")?.ToString() ?? null;


			var t_fecha_texto = GetTipoTexto(tipo_fecha);
			var filtros = GetCadenaDeEstadosSeleccionados(solicitud);
			titulo = $"Listado de Cheques {t_fecha_texto} desde el {fDesdePrint} hasta el {fHastaPrint} {filtros}";

			return _financieroServicio.GetFinancieroBcoVencChequeEmitidoLista(new FinancieroBcoVencChequeEmitidoListaRequest() 
			{ 
				id_f = id_f,
				ctaf_id = ctaf_id,
				id_c = id_c,
				cta_id = cta_id,
				id_u = id_u,
				usu_id = usu_id,
				tipo_fecha = Convert.ToChar(tipo_fecha),
				desde = Convert.ToDateTime(desde),
				hasta = Convert.ToDateTime(hasta),
				estado = estado,
			});
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<FinancieroBcoVencChequeEmitidoListaDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<FinancieroBcoVencChequeEmitidoListaDto> registros = ObtenerDatos(solicitud, out tit);

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
