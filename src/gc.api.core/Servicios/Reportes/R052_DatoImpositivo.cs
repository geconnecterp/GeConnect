using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Impositivo;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.api.core.Servicios.Reportes
{
    public class R052_DatoImpositivo : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiProductoServicio _prodSv;

        private readonly EmpresaGeco _empresaGeco;
        private List<string> _titulos;
        private List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R052_DatoImpositivo(IUnitOfWork uow, IApiProductoServicio servicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _prodSv = servicio;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Producto","Imp.","II" };//agregar los nombres de las listas
            _campos = new List<string> { "p_id", "p_desc", "infoImp", "in_alicuota" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            float[] anchos;

            PdfWriter? writer = null;
            Document pdf;
            string strSubtitulo = string.Empty;
            try
            {
                var ms = new MemoryStream();
                #region Obteniendo registros desde la base de datos
                string tit;
                List<ImpositivoDatoDto> registros = ObtenerDatos(solicitud, out tit);
                
                solicitud.Titulo = tit;

                solicitud.SubTitulo = strSubtitulo;


                //ordenando registros por rubro y proveedor
                registros = registros.OrderBy(r => r.sec_id).ThenBy(r => r.rub_id).ToList();

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
                anchos = [15f, 55f, 15f, 15f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var chicoBold = HelperPdf.FontChicoPredeterminado(true);
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera               
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

                var niveles = new List<NivelAgrupamiento> {
                    new NivelAgrupamiento
                    {
                        CampoGrupo = "sec_id",
                        CampoDescripcion="sec_desc",
                        Nivel=0,
                        ColorFondo = new BaseColor(180, 180, 180),
                        MostrarSubtotal = false,
                        EtiquetaSubtotal = null
                    },
                    new NivelAgrupamiento
                    {
                        CampoGrupo = "rub_id",
                        CampoDescripcion="rub_desc",
                        Nivel=1,
                        ColorFondo = new BaseColor(220, 220, 220),
                        MostrarSubtotal = false,
                        EtiquetaSubtotal = null
                    }
                };

                #region Carga del Listado
                List<string> camposTotalizables = [];
                HelperPdf.GenerarListadoAgrupado(pdf,
                    registros,
                    _campos,
                    _titulos,
                    anchos,
                    niveles, chico, normal, null, false, null);
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
                _logger.LogError(ex, "Error en R049");
                throw new NegocioException("Se produjo un error al intentar generar el Reporte de Precios. Para mayores datos ver el log.");
            }
        }



        private List<ImpositivoDatoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
        {
            //primero debemos buscar los canales para poder loopera y traer todas las
            //ofertas que no estan activas
            List<ImpositivoDatoDto> datos = [];

            //var qf = solicitud.Parametros.GetValueOrDefault("json_p", "{}").ToString() ?? "{}";
            var rel01 = solicitud.Parametros.GetValueOrDefault("StrOpt03", "")?.Split(',').ToList();
            var rel02 = solicitud.Parametros.GetValueOrDefault("StrOpt04", "")?.Split(',').ToList();
            var rel03 = solicitud.Parametros.GetValueOrDefault("StrOpt01", "")?.ToString() ?? "";

            var conII = solicitud.Parametros.GetValueOrDefault("Opt1", "false").ToBoolean();
            var condIva = solicitud.Parametros.GetValueOrDefault("Tipo", "false")?.ToString() ?? "";
            var aliIva = solicitud.Parametros.GetValueOrDefault("Estado", "false")?.ToString() ?? "";
            var adm = solicitud.Parametros.GetValueOrDefault("Adm_id", "")?.ToString() ?? "";
            var usu = solicitud.Parametros.GetValueOrDefault("Usu_id", "")?.ToString() ?? "";

            titulo = solicitud.Titulo;
                        
            datos = _prodSv.ObtenerDatosImpositivos(new QueryFilters
            {
                Rel01 = rel01,
                Rel02 = rel02,
                //Rel03 = rel03.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                Rel03 = string.IsNullOrEmpty(rel03) ? null : rel03.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                //Rel04 = rel04.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                //Rel04 = string.IsNullOrEmpty(rel04) ? null : rel04.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                Opt1 = conII,
                Tipo = condIva,
                Estado = aliIva,
                Usu_id = usu,
                Adm_id = adm
            });

            return datos;
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }
    }
}
