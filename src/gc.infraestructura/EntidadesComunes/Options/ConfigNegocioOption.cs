using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.EntidadesComunes.Options
{
    public class ConfigNegocioOption
    {
        public ConfigNegocioOption()
        {
            Mensajes = new Mensajes();
        }
        public int Identificador { get; set; }
        public int TiempoDuracionToken { get; set; }
        public int ToleranciaDemora { get; set; }
        public string? UrlCotizacionDolar { get; set; }
        public string? PatronFiltrado { get; set; }
        public Mensajes Mensajes { get; set; }
        public string? SMSUrl { get; set; }
        public string? SMSUser { get; set; }
        public string? SMSPass { get; set; }
        public bool SMSActivado { get; set; }
        public bool MailActivado { get; set; }
        public string? MailOrigen { get; set; }
        public string? MailClave { get; set; }
        public int SmtpPuerto { get; set; }
        public string? SmtpUrl { get; set; }
        public string? SmsWebServiceUrl { get; set; }
        public string? SmsUsuario { get; set; }
        public string? SmsClave { get; set; }
        public string? RutaRepositorioPDF { get; set; }

    }

    public class Mensajes
    {
        public Mensajes()
        {
            Msj01 = Msj02 = Msj03 = Msj04 = "";
        }
        public string Msj01 { get; set; }
        public string Msj02 { get; set; }
        public string Msj03 { get; set; }
        public string Msj04 { get; set; }
    }
}
