namespace gc.infraestructura.Dtos.DocManager
{
    public class DatosCabeceraDto
    {
        public string NombreEmpresa { get; set; }= string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string IIBB { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string TituloDocumento { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Sucursal { get; set; } = string.Empty;
    }

    public class DatosCuerpoDto<T>
    {
        public string CtaId { get; set; }   = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Domicilio { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public List<T> Datos { get; set; }= new List<T>();
        //public List<string> Titulos { get; set; } = new List<string>();
        //public List<string> Columnas { get; set; } = new List<string>();
        //public List<float> ColumnasAncho { get; set; } = new List<float>();
    }

    public class DatosPieDto
    {
        public string Observaciones { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;     
    }

    public class PrintRequestDto<T>
    {
        public TipoEjecucion TipoEjecucion { get; set; } 
        public Modulo ModuloImpresion { get; set; }
        public DatosCabeceraDto Cabecera { get; set; } = new DatosCabeceraDto();
        public DatosCuerpoDto<T> Cuerpo { get; set; } = new DatosCuerpoDto<T>();
        public DatosPieDto Pie { get; set; } = new DatosPieDto();
    }

    public enum TipoEjecucion
    {
        None = 0,
        PDF = 1,
        EMAIL = 2,
        TXT = 3,
        WHATSAPP = 4,
        EXCEL = 5,
        PDF_IMPRESION = 6,
        PDF_EMAIL = 7,
        PDF_WHATSAPP = 8,
        PDF_EMAIL_WHATSAPP = 9,
        PDF_IMPRESION_EMAIL = 10,
        PDF_IMPRESION_WHATSAPP = 11,
        PDF_IMPRESION_EMAIL_WHATSAPP = 12,
        TXT_IMPRESION = 13,
        TXT_EMAIL = 14,
        TXT_WHATSAPP = 15,
        TXT_EMAIL_WHATSAPP = 16,
        TXT_IMPRESION_EMAIL = 17,
        TXT_IMPRESION_WHATSAPP = 18,
        TXT_IMPRESION_EMAIL_WHATSAPP = 19,
        EXCEL_IMPRESION = 20,
        EXCEL_EMAIL = 21,
        EXCEL_WHATSAPP = 22,
        EXCEL_EMAIL_WHATSAPP = 23,
        EXCEL_IMPRESION_EMAIL = 24,
        EXCEL_IMPRESION_WHATSAPP = 25,
        EXCEL_IMPRESION_EMAIL_WHATSAPP = 26,

    }

    public enum Modulo
    {
        CUENTA_CORRIENTE = 1,
        ORDEN_DE_PAGO = 2,
        CERTIFICADO_DE_RETENCION_GANANCIA = 3,
        CERTIFICADO_DE_RETENCION_IIBB = 4,
        ORDEN_DE_PAGO_DUPLICADO = 5,
        VENCIMIENTO_COMPROBANTES = 6,
        COMPROBANTES = 7,
        RECEPCION_PROVEEDORES = 8,
    }


}