using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R050_ReporteStock : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R050_ReporteStock(IUnitOfWork uow, IConsultaServicio consulta, IFinancieroServicio financieroServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;

			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
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
				int agrp;
				List<ProductoStkDto> registros = ObtenerDatos(solicitud, out tit, out agrp);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = ObtenerSubtitulo(registros);

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
				HelperPdf.CargarProductosParaRptDeStk(pdf, registros, agrp, chico, normalBold);
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

		private List<ProductoStkDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out int agrupador)
		{
			var lSuc_str = solicitud.Parametros.GetValueOrDefault("lSuc", "")?.ToString() ?? "";
			var lDep_str = solicitud.Parametros.GetValueOrDefault("lDep", "")?.ToString() ?? "";
			var lProv_str = solicitud.Parametros.GetValueOrDefault("lProv", "")?.ToString() ?? "";
			var lFam_str = solicitud.Parametros.GetValueOrDefault("lFam", "")?.ToString() ?? "";
			var lRub_str = solicitud.Parametros.GetValueOrDefault("lRub", "")?.ToString() ?? "";

			var lSuc_lst = lSuc_str.Trim().Length == 0 ? [] : lSuc_str.Split(',').ToList();
			var lDep_lst = lDep_str.Trim().Length == 0 ? [] : lDep_str.Split(',').ToList();
			var lProv_lst = lProv_str.Trim().Length == 0 ? [] : lProv_str.Split(',').ToList();
			var lFam_lst = lFam_str.Trim().Length == 0 ? [] : lFam_str.Split(',').ToList();
			var lRub_lst = lRub_str.Trim().Length == 0 ? [] : lRub_str.Split(',').ToList();

			cant_Dep = lDep_lst.Count;
			cant_Suc = lSuc_lst.Count;
			cant_Fam = lFam_lst.Count;
			cant_Rub = lRub_lst.Count;
			cant_Prov = lProv_lst.Count;

			var chkStkPos = GetBoolParam(solicitud.Parametros, "chkStkPos");
			var chkStkCero = GetBoolParam(solicitud.Parametros, "chkStkCero");
			var chkStkNeg = GetBoolParam(solicitud.Parametros, "chkStkNeg");
			var chkEstAct = GetBoolParam(solicitud.Parametros, "chkEstAct");
			var chkEstDisc = GetBoolParam(solicitud.Parametros, "chkEstDisc");

			var agrp = solicitud.Parametros.GetValueOrDefault("agrupador", "")?.ToString() ?? null;

			titulo = $"Listado de Stock de Productos";
			agrupador = Convert.ToInt32(agrp);

			return _consultaServicio.ConsultarProductoStk(new ConsultarStockRequest()
			{
				lSuc = lSuc_lst,
				lDep = lDep_lst,
				lProv = lProv_lst,
				lFam = lFam_lst,
				lRub = lRub_lst,
				chkStkPos = chkStkPos,
				chkStkCero = chkStkCero,
				chkStkNeg = chkStkNeg,
				chkEstAct = chkEstAct,
				chkEstDisc = chkEstDisc,
				agrupador = Convert.ToInt32(agrp),
				Registros = 999999999,
				Pagina = 1
			});
		}

		private static int cant_Suc = 0;
		private static int cant_Dep = 0;
		private static int cant_Prov = 0;
		private static int cant_Fam = 0;
		private static int cant_Rub = 0;

		private static string ObtenerSubtitulo(List<ProductoStkDto> registros)
		{
			var subTit = string.Empty;
			if (registros == null || registros.Count == 0)
				return subTit;
			var proveedores = cant_Prov > 0
				? string.Join(",", registros.GroupBy(x => x.cta_id).Select(g => g.First().cta_denominacion).ToList())
				: "Todos";
			var rubros = cant_Rub > 0
				? string.Join(",", registros.GroupBy(x => x.rub_id).Select(g => g.First().rub_desc).ToList())
				: "Todos";
			var depositos = registros.First().titulo_repo;
			subTit = $"Depósitos: {depositos}\nProveedor: {proveedores}\nRubros: {rubros}";
			return subTit;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			int agrp;
			List<ProductoStkDto> registros = ObtenerDatos(solicitud, out tit, out agrp);

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
			int agrp;
			List<ProductoStkDto> registros = ObtenerDatos(solicitud, out tit, out agrp);

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
