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
	}
}
