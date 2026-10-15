namespace gc.infraestructura.Dtos.Libros;

public class IvaVentasDetalleDto
{
    public int total_registros { get; set; }
    public int total_paginas { get; set; }
    public string tipo { get; set; } = "";
    public string tipo_desc { get; set; } = "";
    public string pv { get; set; } = "";
    public string compte { get; set; } = "";
    public string repetido { get; set; } = "";
    public string suc { get; set; } = "";
    public DateTime? fecha { get; set; }
    public string cta_id { get; set; } = "";
    public string cuit { get; set; } = "";
    public string nombre { get; set; } = "";
    public string cond_afip { get; set; } = "";
    public decimal? gra_2700 { get; set; }
    public decimal? iva_2700 { get; set; }
    public decimal? gra_2100 { get; set; }
    public decimal? iva_2100 { get; set; }
    public decimal? gra_1050 { get; set; }
    public decimal? iva_1050 { get; set; }
    public decimal? no_gravado { get; set; }
    public decimal? exento { get; set; }
    public decimal? ii { get; set; }
    public decimal? per_ib { get; set; }
    public decimal? per_lh { get; set; }
    public decimal? per_otros { get; set; }
    public string ib_nro { get; set; } = "";
    public string ib_regimen { get; set; } = "";
    public decimal? total { get; set; }
    public string cae { get; set; } = "";
    public DateTime? cm_cae_vto { get; set; }
    public string caja_parte { get; set; } = "";
    public string caja_cierre { get; set; } = "";
}

public class IvaVentasPagina
{
    public List<IvaVentasDetalleDto> Filas { get; set; } = [];
    public int Total { get; set; }
    public int Paginas { get; set; }
    public int Pagina { get; set; }
    public int Registros { get; set; }
}

public record IvaOpcion(string Id, string Texto);
public class IvaVentasOpciones
{
    public List<IvaOpcion> Sucursales { get; set; } = [];
    public List<IvaOpcion> Comprobantes { get; set; } = [];
    public List<IvaOpcion> Cajas { get; set; } = [];
}

// Un único orden explícito para la grilla y el Excel. No incluye metadatos de paginación.
public static class IvaVentasColumnas
{
    public static readonly (string Campo, string Titulo)[] Todas =
    [
        ("suc", "Sucursal"), ("fecha", "Fecha"), ("tipo", "Tipo"), ("tipo_desc", "Comprobante"),
        ("pv", "Punto de venta"), ("compte", "Número"), ("repetido", "Repetido"),
        ("cta_id", "Cuenta"), ("nombre", "Razón social"), ("cuit", "CUIT"), ("cond_afip", "Condición AFIP"),
        ("no_gravado", "No gravado"), ("exento", "Exento"),
        ("gra_2700", "Gravado 27%"), ("iva_2700", "IVA 27%"),
        ("gra_2100", "Gravado 21%"), ("iva_2100", "IVA 21%"),
        ("gra_1050", "Gravado 10,5%"), ("iva_1050", "IVA 10,5%"),
        ("per_ib", "Percepción IB"), ("per_lh", "Percepción LH"), ("per_otros", "Otras percepciones"),
        ("ii", "Impuestos internos"), ("total", "Total"),
        ("ib_nro", "Número IB"), ("ib_regimen", "Régimen IB"), ("cae", "CAE"),
        ("cm_cae_vto", "Vencimiento CAE"), ("caja_parte", "Parte de caja"), ("caja_cierre", "Cierre de caja")
    ];
}
