namespace gc.infraestructura.Dtos.Libros;

// Contrato explícito de SPGECO_IVA_C_D y SF_CCB_Compte_Comp; no inferir columnas de b.*.
public class IvaComprasDetalleDto
{
    public int total_registros { get; set; }
    public int total_paginas { get; set; }
    public string tipo_prov { get; set; } = "";
    public string tipo { get; set; } = "";
    public string tipo_desc { get; set; } = "";
    public string compte { get; set; } = "";
    public string movi { get; set; } = "";
    public string cta_id { get; set; } = "";
    public string cuit { get; set; } = "";
    public string nombre { get; set; } = "";
    public string cond_afip { get; set; } = "";
    public string cm_cae { get; set; } = "";
    public string ccb_27 { get; set; } = "";
    public string ccb_21 { get; set; } = "";
    public string ccb_105 { get; set; } = "";
    public string ccb_otro { get; set; } = "";
    public string ccb_exe { get; set; } = "";
    public string ccb_ng { get; set; } = "";
    public string ccb_mt { get; set; } = "";
    public DateTime? fecha { get; set; }
    public DateTime? carga { get; set; }
    public DateTime? recepcion { get; set; }
    public DateTime? cm_cae_vto { get; set; }
    public decimal? gra_2700 { get; set; }
    public decimal? iva_2700 { get; set; }
    public decimal? gra_2100 { get; set; }
    public decimal? iva_2100 { get; set; }
    public decimal? gra_1050 { get; set; }
    public decimal? iva_1050 { get; set; }
    public decimal? gra_otro { get; set; }
    public decimal? iva_otro { get; set; }
    public decimal? no_gravado { get; set; }
    public decimal? exento { get; set; }
    public decimal? otro_ng { get; set; }
    public decimal? mt { get; set; }
    public decimal? ii { get; set; }
    public decimal? percep_iva { get; set; }
    public decimal? percep_ib { get; set; }
    public decimal? percep_lh { get; set; }
    public decimal? percep_otros { get; set; }
    public decimal? total { get; set; }
}

public class IvaComprasPagina
{
    public List<IvaComprasDetalleDto> Filas { get; set; } = [];
    public int Total { get; set; }
    public int Paginas { get; set; }
    public int Pagina { get; set; }
    public int Registros { get; set; }
}

public class IvaComprasOpciones
{
    public List<IvaOpcion> TiposProveedor { get; set; } = [];
    public List<IvaOpcion> Comprobantes { get; set; } = [];
}

// Un solo orden de columnas para grilla y Excel; sin metadatos de paginación.
public static class IvaComprasColumnas
{
    public static readonly (string Campo, string Titulo)[] Todas =
    [
        ("fecha", "Fecha"),
        ("tipo", "Tipo"),
        ("tipo_desc", "Comprobante"),
        ("compte", "Número"),
        ("tipo_prov", "Tipo de proveedor"),
        ("cta_id", "Cuenta"),
        ("nombre", "Razón social"),
        ("cuit", "CUIT"),
        ("cond_afip", "Condición AFIP"),
        ("no_gravado", "No gravado"),
        ("exento", "Exento"),
        ("gra_2700", "Gravado 27%"),
        ("iva_2700", "IVA 27%"),
        ("gra_2100", "Gravado 21%"),
        ("iva_2100", "IVA 21%"),
        ("gra_1050", "Gravado 10,5%"),
        ("iva_1050", "IVA 10,5%"),
        ("gra_otro", "Gravado otras alícuotas"),
        ("iva_otro", "IVA otras alícuotas"),
        ("otro_ng", "Otros no gravados"),
        ("mt", "Monotributo / no inscripto"),
        ("ii", "Impuestos internos"),
        ("percep_iva", "Percepción IVA"),
        ("percep_ib", "Percepción IB"),
        ("percep_lh", "Percepción LH"),
        ("percep_otros", "Otras percepciones"),
        ("total", "Total"),
        ("cm_cae", "CAE / CAI"),
        ("cm_cae_vto", "Vencimiento CAE / CAI"),
        ("movi", "Movimiento"),
        ("carga", "Fecha de carga"),
        ("recepcion", "Fecha de recepción"),
        ("ccb_27", "Cuenta contable 27%"),
        ("ccb_21", "Cuenta contable 21%"),
        ("ccb_105", "Cuenta contable 10,5%"),
        ("ccb_otro", "Cuenta contable otras"),
        ("ccb_exe", "Cuenta contable exento"),
        ("ccb_ng", "Cuenta contable no gravado"),
        ("ccb_mt", "Cuenta contable monotributo"),
    ];
}
