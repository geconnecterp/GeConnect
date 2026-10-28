using gc.infraestructura.Dtos.Libros;
using static gc.infraestructura.Dtos.Libros.CertificadosSufridosConsulta;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public static class CertificadosSufridosPresentacion
{
    public static object Crear(CertificadosSufridosConsulta consulta, CertificadosSufridosFiltro filtro)
    {
        consulta.ValidarModo(filtro);
        if (filtro.AgrupadoPorCuit) return CrearPorCuit(consulta, filtro);
        var ib = filtro.Impuesto == "IB";
        var columnas = new List<object>();
        void Col(string titulo, bool numero = false, bool textoLargo = false) => columnas.Add(new { titulo, numero, textoLargo });
        Col("Fecha"); Col("Origen", textoLargo: true); Col("Tipo de comprobante", textoLargo: true); Col("Comprobante / Certificado");
        Col("Cuenta"); Col("Razón social", textoLargo: true); Col("CUIT"); Col("Base imponible", true); Col("Alícuota (SP)", true); Col("Retención / Percepción", true);
        if (ib) { Col("Alícuota LH (SP)", true); Col("Importe LH", true); Col("Total con LH", true); }
        Col("Tipo", textoLargo: true); Col("Domicilio", textoLargo: true);
        var filas = new List<object>(); var grupos = new List<object>();
        foreach (var grupo in consulta.Grupos(filtro))
        {
            var id = grupos.Count;
            grupos.Add(new { id, titulo = grupo.Titulo, cantidad = grupo.Filas.Count,
                totalBase = Numero(Sumar(grupo.Filas, f => f.@base)), totalImporte = Numero(Sumar(grupo.Filas, f => f.percepcion)),
                totalLH = ib ? Numero(Sumar(grupo.Filas, f => f.percepcion_lh)) : null,
                registrosOrigen = (string?)null,
                total = Numero(Sumar(grupo.Filas, f => TotalFila(f, ib))) });
            foreach (var f in grupo.Filas)
            {
                var celdas = new List<string> { f.cm_fecha?.ToString("dd/MM/yyyy") ?? "—", Origen(f.origen), Texto(f.tco_desc), Texto(f.cm_compte),
                    Texto(f.cta_id), Texto(f.cm_nombre), Texto(f.cm_cuit), Numero(f.@base), Numero(f.ali), Numero(f.percepcion) };
                if (ib) celdas.AddRange([Numero(f.ali_lh), Numero(f.percepcion_lh), Numero(TotalFila(f, true))]);
                celdas.AddRange([Texto(f.tipo_desc), Texto(f.cm_domicilio)]);
                filas.Add(new { grupo = id, celdas });
            }
        }
        return new { columnas, filas, grupos, total = filas.Count, agrupadoPorCuit = false, registrosOrigen = (string?)null,
            contexto = filtro.Impuesto + " · " + consulta.ImpuestoTexto + " · " + filtro.Descripcion,
            totalBase = Numero(Sumar(consulta.Filas, f => f.@base)), totalImporte = Numero(Sumar(consulta.Filas, f => f.percepcion)),
            totalLH = ib ? Numero(Sumar(consulta.Filas, f => f.percepcion_lh)) : null,
            totalFinal = Numero(Sumar(consulta.Filas, f => TotalFila(f, ib))),
            incompletos = consulta.Filas.Any(f => !f.@base.HasValue || !f.percepcion.HasValue || (ib && !f.percepcion_lh.HasValue)) };
    }
    private static object CrearPorCuit(CertificadosSufridosConsulta consulta, CertificadosSufridosFiltro filtro)
    {
        var columnas = new[] {
            new { titulo = "CUIT", numero = false, textoLargo = false },
            new { titulo = "Razón social", numero = false, textoLargo = true },
            new { titulo = "Domicilio", numero = false, textoLargo = true },
            new { titulo = "Impuesto", numero = false, textoLargo = false },
            new { titulo = "Tipo", numero = false, textoLargo = false },
            new { titulo = "Descripción", numero = false, textoLargo = true },
            new { titulo = "Registros de origen", numero = true, textoLargo = false },
            new { titulo = "Total sufrido (incluye LH)", numero = true, textoLargo = false }
        };
        var filas = new List<object>(); var grupos = new List<object>();
        foreach (var grupo in consulta.GruposPorCuit())
        {
            var id = grupos.Count;
            var total = Numero(SumarPorCuit(grupo.Filas));
            grupos.Add(new { id, titulo = grupo.Titulo, cantidad = grupo.Filas.Count,
                totalBase = (string?)null, totalImporte = total, totalLH = (string?)null, total,
                registrosOrigen = Cantidad(CantidadPorCuit(grupo.Filas)) });
            foreach (var f in grupo.Filas)
                filas.Add(new { grupo = id, celdas = new[] { Texto(f.cm_cuit), Texto(f.cm_nombre), Texto(f.cm_domicilio), Texto(f.imp_id),
                    Texto(f.tipo), Texto(f.tipo_desc), Cantidad(f.percepcion_cant), Numero(f.percepcion) } });
        }
        var importe = Numero(SumarPorCuit(consulta.FilasPorCuit));
        return new { columnas, filas, grupos, total = filas.Count, agrupadoPorCuit = true,
            registrosOrigen = Cantidad(CantidadPorCuit(consulta.FilasPorCuit)),
            contexto = filtro.Impuesto + " · " + consulta.ImpuestoTexto + " · Agrupado por CUIT · " + filtro.Descripcion,
            totalBase = (string?)null, totalImporte = importe, totalLH = (string?)null, totalFinal = importe,
            incompletos = consulta.FilasPorCuit.Any(f => !f.percepcion.HasValue || !f.percepcion_cant.HasValue) };
    }
}
