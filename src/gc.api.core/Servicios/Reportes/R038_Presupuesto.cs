using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R038_Presupuesto : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiPresupuetoServicio _presupServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        private PresupuestoDto? _presupuesto = null;

        public R038_Presupuesto(IUnitOfWork uow, IApiPresupuetoServicio presup,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _presupServicio = presup;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Nro", "Código", "Producto", "Cantidad", "P.Neto Un.", "P.Venta Un.", "Total" };
            _campos = new List<string> { "pre_item", "p_id", "p_des", "pre_cantidad", "pre_pneto", "pre_pvta", "pre_total" };
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
                List<PresupuestoProductoDto> registros = ObtenerDatos(solicitud, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron productos del presupuestos.");
                }

                registros.ForEach(x => x.pre_total = x.pre_cantidad * x.pre_pvta);

                var importe = registros.Sum(x => x.pre_total);
                var neto = registros.Sum(x => x.pre_pneto);

                //verifico si se logro obtener los datos del presupuesto
                if (_presupuesto == null || string.IsNullOrEmpty(_presupuesto.pre_id))
                {
                    throw new NegocioException("No se encontraron los datos del Presupuesto.");
                }

                ///En esta seccion se busca al cliente, a sus datos. Pero si no existe la cuenta
                ///significa que el cliente es un consumidor final y por lo tanto debería tener
                ///los datos Nombre y Domicilio. Datos obligatorios si no se carga una cuenta.
                List<CuentaDto> c;
                CuentaDto cliente = new CuentaDto();
                if (!string.IsNullOrEmpty(_presupuesto.cta_id))
                { //buscando datos del cliente
                    c = _cuentaSv.GetCuentaComercialLista(_presupuesto.cta_id, 'T');
                    if (c == null || c.Count == 0)
                    {
                        throw new NegocioException($"No se encontraron datos del cliente {_presupuesto.cta_id}.");
                    }
                    cliente = c[0];
                    cliente.Monto = 0m;
                    cliente.MontoEtiqueta = "";
                }
                else
                {
                    cliente.Cta_Id = "C0000000";
                    cliente.Cta_Denominacion = _presupuesto.pre_nombre;
                    cliente.Cta_Domicilio = _presupuesto.pre_domicilio;
                }

                //COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
                //tit += $" {cliente.Cta_Denominacion}" ;
                solicitud.Titulo = tit;
                solicitud.Cuenta = cliente;

                ////hago el modelo de dato aca ya que necesito los datos de la cuenta
                //var regs = registros.Select(x => new
                //{
                //    x.Grupo,
                //    GrDesc = x.Grupo_des,
                //    Descripcion = x.Concepto,
                //    Importe = x.Cc_importe,
                //}).ToList();

                #endregion
                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath ?? "", 20, pdf.PageSize.Height - 10, 15);

                #endregion
                //****=============================****/
                //****  CAMBIAR ANCHOS DE COLUMNAS ****
                //****=============================****/
                anchos = [10f, 10f, 25f, 10f, 15f, 15f, 15f];

                var chico = HelperPdf.FontChicoPredeterminado();
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

                #region Datos del Cliente o Proveedor
                tabla = HelperPdf.GeneraTabla(4, [20f, 70f, 5f, 5f], 100, 10, 10);
                //hay que ir a buscar los datos del cliente para presentarlos en pantalla.
                HelperPdf.CargaDatosPresupuesto(pdf, _presupuesto, normal, normalBold);
                #endregion


                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);
                //utilizo cliente.Monto para el total previamente cargado, pero ya dejo preparado el helper para definir
                //multiples campos con totales. Ejemplo: Debe, Haber y Saldo.
                //var totales = new Dictionary<string, decimal>
                //        {
                //            { "Debe", 15420.50m },
                //            { "Haber", 10325.30m },
                //            { "Saldo", 5095.20m }
                //        };
                var totales = new Dictionary<string, decimal>{
                    { "pre_total", importe},
                    { "pre_pneto", neto }
                };


                //HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico, false, true, totales);
                //var aTotalizar = new List<string> { "Importe" };
                HelperPdf.GenerarListadoDesdeLista(pdf, registros, _campos, anchos, chico, false, true, totales);
                string pesos = HelperGen.EnLetras(importe.ToString());
                //linea con el detalle de el total en palabras
                
                pdf.Add(new Paragraph($"Son Pesos {pesos}", chico));

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
                _logger.LogError(ex, "Error en R038");
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }
        }



        private List<PresupuestoProductoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
        {
            var pre_id = solicitud.Parametros.GetValueOrDefault("pre_id", "").ToString() ?? "";

            var presup = _presupServicio.ObtenerPresupuesto(pre_id);
            if (presup == null || presup.Count() == 0)
            {
                throw new NegocioException($"No se encontró el presupuesto {pre_id}.");
            }
            _presupuesto = presup[0];

            //Se obtienen los parámetros del reporte


            titulo = solicitud.Titulo;
            return _presupServicio.ObtenerDetallePresupuesto(pre_id);

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
            //#region Obteniendo registros desde la base de datos
            //string ctaId;
            //string tit;
            //List<ConsOrdPagosDetDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            //if (registros == null || registros.Count == 0)
            //{
            //    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            //}

            ////hago el modelo de dato aca ya que necesito los datos de la cuenta
            //var regs = registros.Select(x => new
            //{
            //    x.Grupo,
            //    GrDesc = x.Grupo_des,
            //    Descripcion = x.Concepto,
            //    Importe = x.Cc_importe,
            //}).ToList();


            //#endregion

            //return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
            //#region Obteniendo registros desde la base de datos
            //string ctaId;
            //string tit;
            //List<ConsOrdPagosDetDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            //if (registros == null || registros.Count == 0)
            //{
            //    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            //}

            ////hago el modelo de dato aca ya que necesito los datos de la cuenta
            //var regs = registros.Select(x => new
            //{
            //    x.Grupo,
            //    GrDesc = x.Grupo_des,
            //    Descripcion = x.Concepto,
            //    Importe = x.Cc_importe,
            //}).ToList();

            //#endregion

            //return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
