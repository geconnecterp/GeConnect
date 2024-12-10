
namespace gc.api.core.Entidades
{
    public class FormaDePago : EntidadBase
    {
        public string Fp_id { get; set; } = string.Empty;
        public string Fp_desc { get; set; } = string.Empty;
        public string Fp_unidad { get; set; } = string.Empty;
        public char Fp_cliente { get; set; }
        public char Fp_proveedor { get; set; }
        public string Fp_lista { get; set; } = string.Empty;
    }
}
