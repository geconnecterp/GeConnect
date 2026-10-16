using gc.infraestructura.EntidadesComunes.Options;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

// Registro aditivo del módulo: no depende de índices de arrays de appsettings por ambiente.
public static class IvaVentasResumenDocumentos
{
    public const string Modulo = "IVA_VENTAS_RESUMEN";
    public const int Reporte = (int)gc.infraestructura.Enumeraciones.InfoReporte.R104_LibroIvaVentasResumen;
    public static void Configurar(DocsManager docs)
    {
        if (docs.Modulos.Any(m => m.Id == Modulo)) return;
        docs.Modulos.Add(new AppModulo
        {
            Id = Modulo, Titulo = "Libro IVA Ventas - Consulta Resumen", Print = true,
            // Excel directo con MIME/nombre XLSX correctos; sin mensajería ni enlace anónimo.
            Export = false, Email = false, Whatsapp = false,
            Reportes = [new AppReporte { Id = Reporte, Titulos = ["Libro IVA Ventas - Resumen"],
                PermiteEnlacePublico = false, PermiteImpresionGlobal = true }]
        });
    }
}
