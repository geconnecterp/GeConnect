using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R056_Proveedor_Sin_Modificacion_Precio : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiProductoServicio _prodSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R056_Proveedor_Sin_Modificacion_Precio(IUnitOfWork uow, IApiProductoServicio ofertaServicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _prodSv = ofertaServicio;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Descripción", "Fecha"};
            _campos = new List<string> { "codigo", "descripcion", "fecha" };
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
                List<ProvSinModPrecioDto> registros = ObtenerDatos(solicitud, out tit);

                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
                    codigo = x.cta_id,
                    descripcion = x.cta_denominacion,
                    fecha= x.pg_fecha_cambio_precios,                 
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
                anchos = [20f, 60f,20f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera    
                //le agregamos al subtitulo el canal
                //solicitud.SubTitulo = tit;
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



        private List<ProvSinModPrecioDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
        {
            //primero debemos buscar los canales para poder loopera y traer todas las
            //ofertas que no estan activas
            List<ProvSinModPrecioDto> trace = [];
            var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
            
            var modifTrace = _prodSv.ProvSinModPrecio(desde);
            trace.AddRange(modifTrace);

            titulo = solicitud.Titulo;

            return trace;
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string tit;
            List<ProvSinModPrecioDto> registros = ObtenerDatos(solicitud, out tit);

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                codigo = x.cta_id,
                descripcion = x.cta_denominacion,
                fecha = x.pg_fecha_cambio_precios,
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string tit;
            List<ProvSinModPrecioDto> registros = ObtenerDatos(solicitud, out tit);

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                codigo = x.cta_id,
                descripcion = x.cta_denominacion,
                fecha = x.pg_fecha_cambio_precios,
            }).ToList();
            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
