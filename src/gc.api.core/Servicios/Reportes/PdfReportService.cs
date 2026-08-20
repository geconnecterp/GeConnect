using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios.Reportes
{
    public class ReportService : Servicio<EntidadBase>, IReportService
    {
        private readonly Dictionary<InfoReporte, IGeneradorReporte> _generadoresReporte;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork uow, IConsultaServicio consSv,
            IAsientoTemporalServicio asiento,
            IAsientoDefinitivoServicio asientoDefinitivo,
            IApiLMayorServicio apiLMayor,
            IApiLDiarioServicio ldSv,
            IApiSumaSaldoServicio apiBSS,
            IApiBalanceGeneralServicio apiBgr,
            IApiProductoServicio apiProdSv,
			IFinancieroServicio finServ,
            IApiPresupuetoServicio apiPresuSv,
            IApiOfertaServicio ofeSv,
            IApiEtiquetaServicio etiqSv,
            IApiPrecioListaServicio plSv,
            IApiPromoComboServicio cmbSv,
            IInventarioServicio invSv,
			IApiPedidoServicio pedSv,
            IApiOrdenDeRepartoServicio ordRepSv,
            IApiProductoFactServicio apiFactura,
            IOptions<EmpresaGeco> empresa, 
            ICuentaServicio ctaSv, 
            IOrdenDePagoServicio _opSv, 
            IApiVentasServicio ventasSv, 
            ILogger<ReportService> logger) : base(uow)
        {

            // Se inicializa el diccionario de generadores de reportes
            _generadoresReporte = new Dictionary<InfoReporte, IGeneradorReporte>
            {
                { InfoReporte.R001_InfoCtaCte, new R001_InformeCuentaCorriente(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R002_InfoVenc, new R002_InformeVencimiento(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R003_InfoCmpte, new R003_InformeComprobantes(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R004_InfoCmpteDet, new R004_InformeComprobanteDetalle(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R005_InfoOPago, new R005_InformeOPago(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R006_InfoOPagoDet, new R006_InformeOPagoDetalle(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R007_InfoRecProv, new R007_InformeRecepcionProveedor(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R008_InfoRecProvDet, new R008_InformeRecepcionProveedorDetalle(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R009_InfoAsientos, new R009_InformeDeAsientos(uow,asiento,asientoDefinitivo,empresa,ctaSv, logger) },
                { InfoReporte.R010_InfoDetalleAsiento, new R010_DetalleDeAsiento(uow,asiento,empresa,ctaSv, logger) },
                { InfoReporte.R011_LibroMayorContable, new R011_LibroMayorContable(uow,apiLMayor,empresa,ctaSv, logger) },
                { InfoReporte.R012_ResumenLibroMayorContable, new R012_ResumenLibroMayorContable(uow,apiLMayor,empresa,ctaSv, logger) },
                { InfoReporte.R013_LibroDiarioXCuenta, new R013_LibroDiarioXCuenta(uow,ldSv,empresa,ctaSv, logger) },
                { InfoReporte.R014_BalanceSumasSaldos, new R014_BalanceSumasSaldos(uow,apiBSS,empresa,ctaSv, logger) },
                { InfoReporte.R015_LibroDiarioResumen, new R015_LibroDiarioResumen(uow,ldSv,empresa,ctaSv, logger) },
                { InfoReporte.R016_BalanceGeneral, new R016_BalanceGeneral(uow,apiBgr,empresa,ctaSv, logger) },
                { InfoReporte.R017_OrdePagoProveedor, new R017_OrdePagoProveedor(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R018_CertRetIIBB, new R018_CertRetIIBB(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R019_CertRetGA, new R019_CertRetGA(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R020_CertRetIVA, new R020_CertRetIVA(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R021_OrdenDeCompra, new R021_OrdenDeCompra(uow,consSv, apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R023_OrdenDePagoDirecta, new R023_OrdenDePagoDirecta(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R024_ConsultaDeOrdenesDePago, new R024_ConsultaDeOrdenesDePago(uow,consSv, _opSv,empresa,ctaSv, logger) },
				{ InfoReporte.R025_TransferenciaEntreCuentas, new R025_TransferenciaEntreCuentas(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R026_ConsultaDeMovimientoFinanciero, new R026_ConsultaDeMovimientoFinanciero(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R027_VencimientoChequesEmitidos, new R027_VencimientoChequesEmitidos(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R028_LibroBancosDetalle, new R028_LibroBancosDetalle(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R029_LibroBancosResumen, new R029_LibroBancosResumen(uow,consSv, finServ,empresa,ctaSv, logger) },
                { InfoReporte.R030_HistoricoLibro, new R030_HistoricoLibro(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R031_ExtractoBancario, new R031_ExtractoBancario(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R032_OfertasSinActivar, new R032_OfertaSinActivar(uow,ofeSv,empresa,ctaSv, logger) },
				{ InfoReporte.R033_ChequePropioEmitido, new R033_ChequePropioEmitido(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R034_ProyeccionDeIngreso, new R034_ProyeccionDeIngreso(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R035_SaldoDeCuentas, new R035_SaldoDeCuentas(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R036_FlujoDeIngresos, new R036_FlujoDeIngresos(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R037_ProyeccionDeEgresos, new R037_ProyeccionDeEgresos(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R038_Presupuesto, new R038_Presupuesto(uow,apiPresuSv,empresa,ctaSv, logger) },
				{ InfoReporte.R039_AnticipoDeEmpleados, new R039_AnticipoDeEmpleado(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R040_DetalleDeAnticipo, new R040_DetalleDeAnticipo(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R041_DetalleDeLiquidacionDeHaberes, new R041_DetalleLiquidacionDeHaberes(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R042_OfertasActivas, new R042_OfertasActivas(uow,ofeSv, empresa,ctaSv, logger) },
				{ InfoReporte.R043_PROMO_COMBO, new R043_CombosActivos(uow,cmbSv, empresa,ctaSv, logger) },
				{ InfoReporte.R044_ConsultaVencPorTipoCtaTipoCompte, new R044_ConsultaVencPorTipoCtaTipoCompte(uow,consSv, finServ,empresa,ctaSv, logger) },
                { InfoReporte.R045_PunteraDeGondola, new R045_PunteraDeGondola(uow,etiqSv,empresa,ctaSv, logger) },
                { InfoReporte.R046_Etiquetas01Precio, new R046_Etiquetas01Precio(uow,etiqSv,empresa,ctaSv, logger) },
                { InfoReporte.R047_Etiquetas02Precio, new R047_Etiquetas02Precios(uow,etiqSv,empresa,ctaSv, logger) },
				{ InfoReporte.R048_ConsultaCertNoRetNoPerc, new R048_ConsultaCertNoRetNoPerc(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R049_REPORTE_PRECIOS, new R049_ReportePrecios(uow,plSv,empresa,ctaSv, logger) },
				{ InfoReporte.R050_REPORTE_STOCK, new R050_ReporteStock(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R051_REPORTE_STOCK_VALORIZADO, new R051_ReporteStockValorizado(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R052_REPORTE_DATO_IMPOSITIVO, new R052_DatoImpositivo(uow,apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R054_REPORTE_STOCK_COMPENSADO, new R054_ReporteStockCompensado(uow,consSv, finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R055_REPORTE_PRECIOS_MENOS, new R055_Modificaciones_Precios_Menos(uow,apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R056_PROVEEDOR_SMP, new R056_Proveedor_Sin_Modificacion_Precio(uow,apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R057_Inv_Repo_Stk_Vs_Conteo, new R057_Inv_Repo_Stk_Vs_Conteo(uow,invSv,empresa,ctaSv, logger) },
				{ InfoReporte.R058_Inv_Repo_Val_X_Sec, new R058_Inv_Repo_Val_X_Sec(uow,invSv,empresa,ctaSv, logger) },
				{ InfoReporte.R059_Inv_Repo_Val_X_Rub, new R059_Inv_Repo_Val_X_Rub(uow,invSv,empresa,ctaSv, logger) },
				{ InfoReporte.R060_Inv_Repo_Val_Detalle, new R060_Inv_Repo_Val_Detalle(uow,invSv,empresa,ctaSv, logger) },
				{ InfoReporte.R061_Inv_Repo_Conteo_X_Usu, new R061_Inv_Repo_Conteo_X_Usu(uow,invSv,empresa,ctaSv, logger) },
				{ InfoReporte.R062_Pedido_De_Cliente, new R062_Pedido_De_Cliente(uow,pedSv,empresa,ctaSv, logger) },
				{ InfoReporte.R063_Orden_De_Reparto_Hoja_De_Ruta, new R063_Orden_De_Reparto_Hoja_De_Ruta(uow,ordRepSv,empresa,ctaSv, logger) },
				{ InfoReporte.R064_Orden_De_Reparto_Hoja_De_Producto, new R064_Orden_De_Reparto_Hoja_De_Producto(uow,ordRepSv,empresa,ctaSv, logger) },
				{ InfoReporte.R065_Pedido_Interno, new R065_Pedido_Interno(uow,apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R066_Pedido_Interno_Listado, new R066_Pedido_Interno_Lista(uow,apiProdSv,empresa,ctaSv, logger) },
				{ InfoReporte.R067_FACTURA_A, new R067_FacturaA(uow,apiFactura,empresa,logger) },
				{ InfoReporte.R068_FACTURA_B, new R068_FACTURA_B(uow,apiFactura,empresa,logger) },
				{ InfoReporte.R069_Analisis_Venta_Mensual, new R069_Analisis_Venta_Mensual(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R070_Analisis_Venta_Diario, new R070_Analisis_Venta_Diario(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R071_Analisis_Venta_Op_Vta_Diario, new R071_Analisis_Venta_Op_Vta_Diario(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R072_Analisis_Venta_Sucursal, new R072_Analisis_Venta_Sucursal(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R073_Analisis_Venta_Cierres, new R073_Analisis_Venta_Cierres(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R074_Analisis_Venta_Anual, new R074_Analisis_Venta_Anual(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R075_Analisis_De_Valores_De_Venta_Mes, new R075_Analisis_De_Valores_De_Venta_Mensual(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R076_Analisis_De_Valores_De_Venta_Diario, new R076_Analisis_De_Valores_De_Venta_Diario(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R077_Analisis_De_Valores_De_Venta_Pv, new R077_Analisis_De_Valores_De_Venta_Pv(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R078_Analisis_De_Valores_De_Venta_Cashback, new R078_Analisis_De_Valores_De_Venta_Cashback(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R079_Sorteo_Comprobantes_Consulta, new R079_Sorteo_Comprobantes_Consulta(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R081_Reporte_Rendicion_Cierre, new R081_Reporte_Rendicion_Cierre(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R082_Reporte_Analitico_Operacion, new R082_Reporte_Analitico_Operacion(uow,ventasSv,empresa,ctaSv, logger) },
				{ InfoReporte.R083_Cons_Cta_Corriente_Financiera, new R083_Cons_Cta_Corriente_Financiera(uow,finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R084_Detalle_Valores_En_Cartera, new R084_Detalle_Valores_En_Cartera(uow,finServ,empresa,ctaSv, logger) },
				{ InfoReporte.R085_Reporte_Mov_Cta_Directa, new R085_Reporte_Mov_Cta_Directa(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R086_Saldo_Cta_Distr_Detalle, new R086_Saldo_Cta_Distr_Detalle(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R087_Saldo_Cta_Distr_Resumen, new R087_Saldo_Cta_Distr_Resumen(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R088_Comisiones_Vendedores_Detalle, new R088_Comisiones_Vendedores_Detalle(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R089_Comisiones_Vendedores_Resumen, new R089_Comisiones_Vendedores_Resumen(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R090_Comisiones_Repartidores_Detalle, new R090_Comisiones_Repartidores_Detalle(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R091_Comisiones_Repartidores_Resumen, new R091_Comisiones_Repartidores_Resumen(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R092_Reporte_Ranking_Y_Rentabilidad, new R092_Reporte_Ranking_Y_Rentabilidad(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R093_Reporte_Evo_Ventas_Periodos_Anteriores, new R093_Reporte_Evo_Ventas_Periodos_Anteriores(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R094_Reporte_Var_Vtas_Y_Comp_Ult_Doce_M, new R094_Reporte_Var_Vtas_Y_Comp_Ult_Doce_M(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R095_Reporte_Eval_De_Nivel_De_Servicio, new R095_Reporte_Eval_De_Nivel_De_Servicio(uow,consSv,empresa,ctaSv, logger) },
				{ InfoReporte.R096_Consulta_Movimiento_De_Stock, new R096_Consulta_Movimiento_De_Stock(uow,consSv,empresa,ctaSv, logger) },
			}; 
            _logger = logger;
        }

        public string GenerarReporteFormatoExcel(ReporteSolicitudDto solicitud)
        {
            string base64 = string.Empty;

            if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
            {
                base64 = generador.GenerarXls(solicitud);
            }
            else
            {
                StringBuilder str = new StringBuilder();
                str.Append("No se pudo identificar el XLS a generar.");
                foreach (var param in solicitud.Parametros)
                {
                    str.Append($"{param.Key}: {param.Value}");
                }
                throw new Exception(str.ToString());
            }

            return base64;
        }

        public string GenerarReporteFormatoTxt(ReporteSolicitudDto solicitud)
        {
            string base64 = string.Empty;

            if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
            {
                base64 = generador.GenerarTxt(solicitud);
            }
            else
            {
                StringBuilder str = new StringBuilder();
                str.Append("No se pudo identificar el TXT a generar.");
                foreach (var param in solicitud.Parametros)
                {
                    str.Append($"{param.Key}: {param.Value}");
                }
                throw new Exception(str.ToString());
            }

            return base64;
        }

        public string GenerateReportAsBase64(ReporteSolicitudDto solicitud)
        {
            try
            {
                string base64 = string.Empty;

                if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
                {
                    base64 = generador.Generar(solicitud);
                }
                else
                {
                    using (var ms = new MemoryStream())
                    { //genera un pdf generico
                        Document document = new Document();
                        PdfWriter.GetInstance(document, ms);
                        document.Open();

                        document.Add(new Paragraph("No se pudo identificar el reporte a generar."));
                        foreach (var param in solicitud.Parametros)
                        {
                            document.Add(new Paragraph($"{param.Key}: {param.Value}"));
                        }
                        document.Close();
                        base64 = Convert.ToBase64String(ms.ToArray());
                    }
                }

                return base64;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
