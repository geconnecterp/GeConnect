namespace gc.infraestructura.Dtos.Contabilidad
{
    public class PlanCuentaDto:Dto
    {
        public string id_padre { get; set; } = string.Empty; //campo para trabajar el alta, con la porcion correspondiente al padre sin los ceros
        public string ccb_id { get; set; } = string.Empty;
        public string ccb_desc { get; set; } = string.Empty;
        public string ccb_lista { get; set; } = string.Empty;
        public char ccb_tipo { get; set; }
        private bool esMovimiento { get; set; }//
        public bool EsMovimiento
        {
            get
            {
                if (string.IsNullOrWhiteSpace(char.ToString(ccb_tipo)))
                    return false;
                return ccb_tipo.Equals('M');
            }
            set
            {
                esMovimiento = value;
            }
        }
       
        public string ccb_id_padre { get; set; } = string.Empty;
        public char ccb_ajuste_inflacion { get; set; }
        private bool hayAjusteInflacion { get; set; }//
        public bool HayAjusteInflacion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(char.ToString(ccb_ajuste_inflacion)))
                    return false;
                return ccb_ajuste_inflacion.Equals('S');
            }
            set
            {
                hayAjusteInflacion = value;
            }
        }
        public decimal ccb_saldo { get; set; }
    }
}
