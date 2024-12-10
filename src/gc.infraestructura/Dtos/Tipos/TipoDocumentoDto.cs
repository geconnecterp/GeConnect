
namespace gc.infraestructura.Dtos
{
    public partial class TipoDocumentoDto:Dto
    {
        public TipoDocumentoDto()
        {
            Tdoc_Id = string.Empty;
            Tdoc_Desc = string.Empty;
            Tdoc_Lista = string.Empty;
        }
        public string Tdoc_Id { get; set; }
        public string Tdoc_Desc { get; set; }
        public string Tdoc_Lista { get; set; }
    }
}
