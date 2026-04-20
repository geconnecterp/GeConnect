namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class ClienteRequestDto
    {
        /// <summary>
        /// A = Alta, cualquier otro valor => Update
        /// </summary>
        public string Abm { get; set; } = string.Empty;

        public string UsuId { get; set; } = string.Empty;

        public string AdmId { get; set; } = string.Empty;

        /// <summary>
        /// Por defecto el SP usa '96'
        /// </summary>
        public string TdocId { get; set; } = "96";

        public string CtaDocumento { get; set; } = string.Empty;

        public string CtaNombre { get; set; } = string.Empty;

        public string CtaApellido { get; set; } = string.Empty;

        /// <summary>
        /// Por defecto el SP usa 'M'
        /// </summary>
        public string Sexo { get; set; } = "M";

        public string CtaDomicilio { get; set; } = string.Empty;

        public string CtaCelu { get; set; } = string.Empty;

        public string CtaEmail { get; set; } = string.Empty;
    }
}
