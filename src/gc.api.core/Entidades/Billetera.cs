namespace gc.api.core.Entidades
{
    public partial class Billetera : EntidadBase
    {
        public Billetera()
        {
            Bill_id = string.Empty;
            Bill_desc = string.Empty;
        }

        public string Bill_id { get; set; }
        public string Bill_desc { get; set; }


        public virtual ICollection<BilleteraOrden> Billeteras_ordeness { get; set; }

    }
}
