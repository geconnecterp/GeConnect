using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
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
    public class R049_ReportePrecios : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiPrecioListaServicio _plSv;

        private readonly EmpresaGeco _empresaGeco;
        private List<string> _titulos;
        private List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R049_ReportePrecios(IUnitOfWork uow, IApiPrecioListaServicio servicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _plSv = servicio;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Producto","Barrado","Imp." };//agregar los nombres de las listas
            _campos = new List<string> { "p_id", "p_desc", "p_id_barrado", "infoImp" };
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
                List<PrecioListaDetalleDto> registros = ObtenerDatos(solicitud, out tit, out List<string> titulosLista, out bool verCosto);
                if (verCosto)
                {
                    _titulos.Add("Costo");
                    _campos.Add("p_pcosto");

                    strSubtitulo = "Con Costos";
                }
                //agregamos las listas de precios seleccionadas
                _titulos.AddRange(titulosLista);
                //se agrega los campos segun la cantidad
                for (int i = 0; i < titulosLista.Count; i++)
                {
                    _campos.Add($"p_pvta{i + 1}");
                }

                if (!string.IsNullOrEmpty(strSubtitulo))
                {
                    strSubtitulo += " y ";
                }

                if (titulosLista.Count == 1)
                {
                    strSubtitulo += "Lista: ";
                }
                else
                {
                    strSubtitulo += "Listas: ";
                }

                foreach (var item in titulosLista)
                {
                    strSubtitulo += $"{item}, ";
                }

                solicitud.SubTitulo = strSubtitulo;

                switch (titulosLista.Count)
                {
                    //                      id, desc, ean, imp, costo, lp     id, desc, ean, imp , lp
                    case 1:
                        anchos = verCosto ? [10f, 46f, 10f, 10f, 12f, 12f] : [10f, 58f, 10f, 10f, 12f];
                        break;
                    case 2:
                        anchos = verCosto ? [10f, 34f, 10f, 10f, 12f, 12f, 12f] : [10f, 46f, 10f, 10f, 12f, 12f];
                        break;
                    case 3:
                        anchos = verCosto ? [10f, 22f, 10f, 10f, 12f, 12f, 12f, 12f] : [10f, 34f, 10f, 10f, 12f, 12f, 12f];
                        break;
                    case 4:
                        anchos = verCosto ? [10f, 10f, 10f, 10f, 12f, 12f, 12f, 12f, 12f] : [10f, 22f, 10f, 10f, 12f, 12f, 12f, 12f];
                        break;
                    default:
                        throw new NegocioException("No se pueden generar reportes con más de 4 listas de precios.");

                }

                //ordenando registros por rubro y proveedor
                registros = registros.OrderBy(r => r.rub_desc).ThenBy(r => r.cta_denominacion).ToList();

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
                //anchos = [20f, 60f, 20f];

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
                        CampoGrupo = "rub_id",
                        CampoDescripcion="rub_desc",
                        Nivel=0,
                        ColorFondo = new BaseColor(180, 180, 180),
                        MostrarSubtotal = false,
                        EtiquetaSubtotal = null
                    },
                    new NivelAgrupamiento
                    {
                        CampoGrupo = "cta_id",
                        CampoDescripcion="cta_denominacion",
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



        private List<PrecioListaDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out List<string> titulosLista, out bool verCosto)
        {
            //primero debemos buscar los canales para poder loopera y traer todas las
            //ofertas que no estan activas
            List<PrecioListaDetalleDto> precios = [];
            var listas = _plSv.ObtenerListaPrecios();

            //var qf = solicitud.Parametros.GetValueOrDefault("json_p", "{}").ToString() ?? "{}";
            var rel01 = solicitud.Parametros.GetValueOrDefault("StrOpt03", "")?.Split(',').ToList();
            var rel02 = solicitud.Parametros.GetValueOrDefault("StrOpt04", "")?.Split(',').ToList();
            var rel03 = solicitud.Parametros.GetValueOrDefault("StrOpt01", "")?.ToString() ?? "";
            var rel04 = solicitud.Parametros.GetValueOrDefault("StrOpt02", "")?.ToString() ?? "";
            var fd = solicitud.Parametros.GetValueOrDefault("Date1", "")?.ToString() ?? "";
            var fh = solicitud.Parametros.GetValueOrDefault("Date2", "")?.ToString() ?? "";
            verCosto = solicitud.Parametros.GetValueOrDefault("Opt1", "false").ToBoolean();
            var adm = solicitud.Parametros.GetValueOrDefault("Adm_id", "")?.ToString() ?? "";
            var usu = solicitud.Parametros.GetValueOrDefault("Usu_id", "")?.ToString() ?? "";

            titulo = solicitud.Titulo;
            titulosLista = [];

            //busco las listas
            var lps = _plSv.ObtenerListaPrecios();
            var lpParam = rel04.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lpParam.Count > 0)
            {
                foreach (var lp in lpParam)
                {
                    string desc = lps.First(x => x.lp_id == lp).lp_desc;
                    titulosLista.Add(desc);
                }
            }
            else
            {
                string desc = lps.First(x => x.lp_id == "001").lp_desc;
                titulosLista.Add(desc);
            }

            precios = _plSv.ObtenerDetallePrecios(new QueryFilters
            {
                Rel01 = rel01,
                Rel02 = rel02,
                //Rel03 = rel03.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                Rel03 = string.IsNullOrEmpty(rel03) ? null : rel03.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                //Rel04 = rel04.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                Rel04 = string.IsNullOrEmpty(rel04) ? null : rel04.Split(',').Select(x => new ComboGenDto { Id = x, Descripcion = x }).ToList(),
                Opt1 = verCosto,
                Date1 = fd,
                Date2 = fh,
                Usu_id = usu,
                Adm_id = adm
            });

            return precios;
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
