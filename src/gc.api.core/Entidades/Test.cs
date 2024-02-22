namespace gc.api.core.Entidades
{
    public partial class Test: EntidadBase
    {
        public Test() {
            DatoStr = "";
        }

        public int Id { get; set; }
        public int DatoInt { get; set; }
        public string DatoStr { get; set; }
        public bool DatoBool { get; set; }
    }
}
