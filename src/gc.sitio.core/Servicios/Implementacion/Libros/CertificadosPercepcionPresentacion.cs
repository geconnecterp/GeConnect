using gc.infraestructura.Dtos.Libros;
using System.Globalization;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public static class CertificadosPercepcionPresentacion
{
    public static object Crear(CertificadosPercepcionConsulta consulta, CertificadosPercepcionFiltro filtro)
    {
        var columnas = new List<object>();
        void Col(string titulo, bool numero = false, bool textoLargo = false) => columnas.Add(new { titulo, numero, textoLargo });
        Col("Fecha"); Col("CUIT"); Col("Razón social", textoLargo: true); Col("Tipo"); Col("Descripción", textoLargo: true);
        Col("Letra"); Col("Tipo GECO"); Col("Nro. comprobante"); Col("Base imponible", true); Col("Alícuota %", true); Col("Percepción", true);
        if (filtro.Impuesto == "IB") { Col("Percepción LH", true); Col("Inscripción IB"); }
        Col("Domicilio", textoLargo: true); Col("Tipo original"); Col("Nro. original");
        string Texto(string? t) => string.IsNullOrWhiteSpace(t) ? "—" : t.TrimEnd();
        string Numero(decimal? n) => n?.ToString("N2", CultureInfo.GetCultureInfo("es-AR")) ?? "—";
        var filas = consulta.Filas.Select(f =>
        {
            var celdas = new List<string> { f.fecha?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—", Texto(f.cuit), Texto(f.razon_social),
                Texto(f.tco), Texto(f.tipo_desc), Texto(f.letra), Texto(f.tipo_geco), Texto(f.compte), Numero(f.@base), Numero(f.ali), Numero(f.percepcion) };
            if (filtro.Impuesto == "IB") celdas.AddRange([Numero(f.percepcion_lh), Texto(f.ib_nro)]);
            celdas.AddRange([Texto(f.domicilio), Texto(f.tco_id_ori), Texto(f.cm_compte_ori)]);
            return celdas;
        }).ToList();
        return new { columnas, filas, total = filas.Count, contexto = filtro.Impuesto + " · " + filtro.Descripcion,
            totalBase = CertificadosPercepcionConsulta.Importe(consulta.TotalBase), totalPercepcion = CertificadosPercepcionConsulta.Importe(consulta.TotalPercepcion),
            totalLH = filtro.Impuesto == "IB" ? CertificadosPercepcionConsulta.Importe(consulta.TotalLH) : null,
            incompletos = consulta.ImportesIncompletos || (filtro.Impuesto == "IB" && !consulta.TotalLH.HasValue) };
    }
}
