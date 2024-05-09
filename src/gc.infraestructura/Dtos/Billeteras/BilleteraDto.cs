namespace gc.infraestructura.Dtos.Billeteras
{
    public partial class BilleteraDto
    {
        public BilleteraDto()
        {
            Bill_id = string.Empty;
            Bill_desc = string.Empty;
        }
        public string Bill_id { get; set; }
        public string Bill_desc { get; set; }

    }
}
