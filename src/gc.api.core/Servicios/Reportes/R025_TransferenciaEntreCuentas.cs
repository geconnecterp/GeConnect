using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace gc.api.core.Servicios.Reportes
{
	public class R025_TransferenciaEntreCuentas : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IFinancieroServicio _finSrv;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R025_TransferenciaEntreCuentas(IUnitOfWork uow, IConsultaServicio consulta, IFinancieroServicio finSrv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;
			_finSrv = finSrv;

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
				string subTit;
				string tra_compte;
				List<FinancieroTraRepoDDto> registros = ObtenerDatos(solicitud, out tit, out subTit, out tra_compte);
				List<FinancieroTraRepoCtagDto> registrosCtag = ObtenterDatosCtag(solicitud);

				if (registros == null || registros.Count <= 0)
					throw new NegocioException($"No existen datos relacionados a la transferencia N° {tra_compte}.");

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subTit;

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

				if (registros.First().tra_anulada == 'S')
				{
					writer.PageEvent = new WatermarkPageEvent("ANULADO");
				}

				pdf.Open();

				#region Datos de la Transferencia
				HelperPdf.CargarTablaDatosDeAcuseDeTransferencia_Encabezado(pdf, registros.First(), normal, normalBold, titulo);
				#endregion

				#region Origen
				if (registros.Where(x => x.grupo.Equals(1)).Any())
				{
					HelperPdf.CargarTablaDatosDeAcuseDeTransferencia_Origen(pdf, [.. registros.Where(x=>x.grupo.Equals(1))], normal, normalBold, titulo);
				}
				#endregion

				#region Destino
				if (registros.Where(x => x.grupo.Equals(2)).Any())
				{
					HelperPdf.CargarTablaDatosDeAcuseDeTransferencia_Destino(pdf, [.. registros.Where(x => x.grupo.Equals(2))], normal, normalBold, titulo);
				}
				#endregion

				#region Gastos
				if (registrosCtag.Any())
				{ 
					HelperPdf.CargarTablaDatosDeAcuseDeTransferencia_Ctag(pdf, registrosCtag, normal, normalBold, titulo);
				}
				#endregion

				#region Total
				if (registros.Where(x => x.grupo.Equals(1)).Any())
				{
					HelperPdf.CargarTablaDatosDeAcuseDeTransferencia_Total(pdf, registros, normal, normalBold);
				}
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
				_logger.LogError(ex, "Error en R025");
				throw new NegocioException("Se produjo un error al intentar generar el Reporte de Transferencias. Para mayores datos ver el log.");
			}
		}

		private List<FinancieroTraRepoDDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subTitulo, out string tra_compte)
		{

			var traCompte = solicitud.Parametros.GetValueOrDefault("tra_compte", "").ToString();
			titulo = $"Acuse Registro de Transferencia N° {traCompte}";
			var listaTemp = _finSrv.GetFinancieroTraRepoDDto(traCompte);
			subTitulo = $"Movimiento N° {listaTemp?.FirstOrDefault()?.dia_movi}";
			tra_compte = traCompte;
			return listaTemp;

		}

		private List<FinancieroTraRepoCtagDto> ObtenterDatosCtag(ReporteSolicitudDto solicitud)
		{
			var traCompte = solicitud.Parametros.GetValueOrDefault("tra_compte", "").ToString();
			var listaTemp = _finSrv.GetFinancieroTraRepoCtag(traCompte);
			return listaTemp;
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subTit;
			string traCompte;
			List<FinancieroTraRepoDDto> registros = ObtenerDatos(solicitud, out tit, out subTit, out string tra_compte);

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
			string traCompte;
			List<FinancieroTraRepoDDto> registros = ObtenerDatos(solicitud, out tit, out subTit, out string tra_compte);

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
