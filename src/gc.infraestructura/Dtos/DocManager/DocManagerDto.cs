using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.DocManager
{
    public class DatosCabeceraDto
    {
        public string NombreEmpresa { get; set; }
        public string CUIT { get; set; }
        public string IIBB { get; set; }
        public string Direccion { get; set; }
        public string TituloDocumento { get; set; }
        public byte[] Logo { get; set; } = null;
        public string Sucursal { get; set; }
    }

    public class DatosCuerpoDto<T>
    {
        public string CtaId { get; set; }
        public string RazonSocial { get; set; }
        public string Domicilio { get; set; }
        public string CUIT { get; set; }
        public string Contacto { get; set; }
        public List<T> Datos { get; set; }
    }

    public class DatosPieDto
    {
        public string Observaciones { get; set; }
        public string Fecha { get; set; }
        public string Usuario { get; set; }
    }

    public class PrintRequestDto<T>
    {
        public TipoEjecucion TipoEjecucion { get; set; }
        public Modulo ModuloImpresion { get; set; }
        public DatosCabeceraDto Cabecera { get; set; }
        public DatosCuerpoDto<T> Cuerpo { get; set; }
        public DatosPieDto Pie { get; set; }
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
    }


}