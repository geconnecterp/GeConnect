using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R042_OfertasActivas : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiOfertaServicio _ofSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R042_OfertasActivas(IUnitOfWork uow, IApiOfertaServicio ofertaServicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _ofSv = ofertaServicio;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Descripción", "Stk","Mg.Vta","P.Vta","Mg Of.","P.Of","V.Desde","V.Hasta" };
            _campos = new List<string> { "codigo", "descripcion", "stk","mgvta","pvta","mgof","pof","vd","vh" };
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
                List<OfertaDto> registros = ObtenerDatos(solicitud, out tit);

                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
                    codigo = x.p_id,
                    descripcion = x.p_desc,
                    stk= x.ps_stk,
                    mgvta=x.p_margen,
                    pvta = x.p_pvta,
                    mgof =x.p_margen_oferta,
                    pof=x.p_pvta_oferta,
                    vd=x.po_fecha_desde,
                    vh=x.po_fecha_hasta
                }).ToList();

                #endregion
                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 15);

                #endregion
                //****=============================****/
                //****  CAMBIAR ANCHOS DE COLUMNAS ****
                //****=============================****/
                anchos = [7f, 34f,5f, 7f, 10f, 7f, 10f, 10f, 10f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera    
                //le agregamos al subtitulo el canal
                solicitud.SubTitulo = tit;
                PdfPTable tabla = GeneraCabeceraPdf(solicitud, logo, chico, titulo, _empresaGeco);
                Phrase phrase = new Phrase();
                phrase.Add(tabla);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                };

                pdf.Header = header;
                #endregion

                pdf.Open();

                //#region Datos del Cliente o Proveedor
                //tabla = HelperPdf.GeneraTabla(4, [20f, 70f, 5f, 5f], 100, 10, 10);
                ////hay que ir a buscar los datos del cliente para presentarlos en pantalla.
                //HelperPdf.CargarTablaClienteProveedor(pdf, c[0], normal, normalBold);
                //#endregion

                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);
                
                HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico);
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
                //_logger.Log(typeof(R001_InformeCuentaCorriente), Level.Error, $"Error al generar el informe de cuenta corriente: {ex.Message}", ex);
                _logger.LogError(ex, "Error en R042");
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }
        }



        private List<OfertaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
        {
            //primero debemos buscar los canales para poder loopera y traer todas las
            //ofertas que no estan activas
            List<OfertaDto> ofertas = [];
            var admId = solicitud.Parametros.GetValueOrDefault("adm_id", "").ToString() ?? "";
            var lpId = solicitud.Parametros.GetValueOrDefault("lp_id", "").ToString() ?? "";
            var canal = solicitud.Parametros.GetValueOrDefault("canal", "").ToString() ?? "";

            var ofertasCanal = _ofSv.ObtenerOfertas(admId, lpId, false);
            ofertas.AddRange(ofertasCanal);

            titulo = canal;

            return ofertas;
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string tit;
            List<OfertaDto> registros = ObtenerDatos(solicitud, out tit);

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                codigo = x.p_id,
                descripcion = x.p_desc,
                stk = 0,
                mgvta = x.p_margen,
                pvta = x.p_pvta,
                mgof = x.p_margen_oferta,
                pof = x.p_pvta_oferta,
                vd = x.po_fecha_desde,
                vh = x.po_fecha_hasta
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string tit;
            List<OfertaDto> registros = ObtenerDatos(solicitud, out tit);

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                codigo = x.p_id,
                descripcion = x.p_desc,
                stk = 0,
                mgvta = x.p_margen,
                pvta = x.p_pvta,
                mgof = x.p_margen_oferta,
                pof = x.p_pvta_oferta,
                vd = x.po_fecha_desde,
                vh = x.po_fecha_hasta
            }).ToList();
            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
