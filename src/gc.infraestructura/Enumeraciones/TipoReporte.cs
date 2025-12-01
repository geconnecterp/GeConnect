using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Enumeraciones
{

    /// <summary>
    /// Enumeración para los tipos de reportes disponibles en el sistema.
    /// Consultarnos antes de ingresar para evitar solapar numeración
    /// </summary>
    public enum InfoReporte
    {
        R001_InfoCtaCte = 1,
        R002_InfoVenc = 2,
        R003_InfoCmpte = 3,
        R004_InfoCmpteDet = 4,
        R005_InfoOPago = 5,
        R006_InfoOPagoDet = 6,
        R007_InfoRecProv = 7,
        R008_InfoRecProvDet = 8,
        R009_InfoAsientos = 9,
        R010_InfoDetalleAsiento = 10,
        R011_LibroMayorContable = 11,
        R012_ResumenLibroMayorContable = 12,
        R013_LibroDiarioXCuenta = 13,
        R014_BalanceSumasSaldos = 14,
        R015_LibroDiarioResumen = 15,
        R016_BalanceGeneral = 16,
        R017_OrdePagoProveedor = 17,
        R018_CertRetIIBB = 18,
        R019_CertRetGA = 19,
        R020_CertRetIVA = 20,
        R021_OrdenDeCompra = 21,
        R023_OrdenDePagoDirecta = 23,
        R024_ConsultaDeOrdenesDePago = 24,
        R025_TransferenciaEntreCuentas = 25,
        R026_ConsultaDeMovimientoFinanciero = 26,
        R027_VencimientoChequesEmitidos = 27,
        R028_LibroBancosDetalle = 28,
        R029_LibroBancosResumen = 29,
        R030_HistoricoLibro = 30,
        R031_ExtractoBancario = 31,
        R032_OfertasSinActivar = 32,
        R033_ChequePropioEmitido = 33,
        R034_ProyeccionDeIngreso = 34,
        R035_SaldoDeCuentas = 35,
        R036_FlujoDeIngresos = 36,
        R037_ProyeccionDeEgresos = 37,
        R038_Presupuesto = 38,
        R039_AnticipoDeEmpleados = 39,
        R040_DetalleDeAnticipo = 40,
        R041_DetalleDeLiquidacionDeHaberes = 41,
        R042_OfertasActivas = 42,
        R043_PROMO_COMBO = 43,
        R044_ConsultaVencPorTipoCtaTipoCompte = 44,
        R045_PunteraDeGondola = 45,
        R046_Etiquetas01Precio = 46,
        R047_Etiquetas02Precio = 47,
		R048_ConsultaCertNoRetNoPerc = 48, 
        R049_REPORTE_PRECIOS = 49,
	}
}
