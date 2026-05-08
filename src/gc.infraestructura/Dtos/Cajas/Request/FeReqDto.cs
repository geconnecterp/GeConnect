namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class FeReqDto
    {
        //{"tco_letra":"A","tco_id":"007","cm_compte":"0001-00000001","cm_repetido":"0"}
        public string tco_letra { get; set; }= string.Empty;
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;
        public string cm_repetido { get; set; } = string.Empty;
    }
}
