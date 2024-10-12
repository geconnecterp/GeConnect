namespace gc.infraestructura.Dtos.Almacen.Rpr
{
    public class ProductoGenDto
    {   //                                                                                                              CTRL        sp
        public int item { get; set; }//0                                                                                            X
        //public string Ope { get; set; }                   
        public string? rp { get; set; }//0                                                                               
        public string ti { get; set; }//                                                                                X           X
        public string? ul_id { get; set; }//0                                                                            x
        //public string Nro_auto { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;//0                                                             X           X
        public string? p_id_prov { get; set; }//0                                                                                    X
        public string? p_id_barrado { get; set; }//0                                                                                 X
        public string p_id_desc { get; set; } = string.Empty;//0                                                           X
        public string up_id { get; set; } = string.Empty; //= 00 => decimal -> acepto lo que traiga (3 decimales)       X
        //public string Cta_id { get; set; } = string.Empty;    
        public string usu_id { get; set; } = string.Empty;//0                                                           
        public int unidad_pres { get; set; } //0                                                                        x
        public int bulto { get; set; } //0                                                                              x
        public decimal us { get; set; }//0                                                                              x
        public DateTime? vto { get; set; }//0                                                                           x
        public decimal cantidad { get; set; }//0                                                                        x
        public decimal cantidad_total { get; set; }//0                                                                  x
        public decimal diferencia { get; set; }//0                                                                      x
        public string? adm_min_excluye { get; set; }//                                                                   x
        public string? adm_may_excluye { get; set; }//                                                                   x                                                                     

    }
}
