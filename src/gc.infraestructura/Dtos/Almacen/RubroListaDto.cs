
namespace gc.infraestructura.Dtos.Almacen
{
    public class RubroListaDto : Dto
    {
        public RubroListaDto()
        {
            Rub_Desc=string.Empty;
            Rub_Id =string.Empty;
        }
        public string Rub_Id { get; set; }
        public string Rub_Desc { get; set; }
        public string Rub_Lista { get; set; }
    }
}
