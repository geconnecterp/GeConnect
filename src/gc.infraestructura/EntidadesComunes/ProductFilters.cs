namespace gc.infraestructura.Core.EntidadesComunes
{
    public class ProductFilters : QueryFilters
    {
        public ProductFilters()
        {
            CodigoBarra = "";
        }

        public new bool Todo
        {
            get
            {
                return Id == default && !Date.HasValue && string.IsNullOrWhiteSpace(Search) && PageSize == default && PageNumber == default && Codigo == default && RubroId == default && SubRubroId == default;
            }
        }

        public int Codigo { get; set; }
        public string CodigoBarra { get; set; }
        public int RubroId { get; set; }
        public int SubRubroId { get; set; }
        public int MarcaId { get; set; }
        public int UnidadId { get; set; }

    }
}
