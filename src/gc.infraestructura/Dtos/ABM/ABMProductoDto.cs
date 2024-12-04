using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.ABM
{
    public class ABMProductoSearchDto : Dto
    {
        public int total_registros { get; set; }
        public int total_paginas { get; set; }
        [Display(Name = "Id")]
        public string P_id { get; set; } = string.Empty;
        [Display(Name = "Descripción")]
        public string P_desc { get; set; } = string.Empty;
        [Display(Name = "Cuenta")]
        public string cta_id { get; set; } = string.Empty;
        [Display(Name = "Denominación")]
        public string Cta_denominacion { get; set; } = string.Empty;
        [Display(Name = "Lista")]
        public string Cta_lista { get; set; } = string.Empty;
        [Display(Name = "Rub Id")]
        public string rub_id { get; set; } = string.Empty;
        [Display(Name = "Rub Desc.")]
        public string rub_desc { get; set; } = string.Empty;
        [Display(Name = "Rub Lista")]
        public string rub_lista { get; set; } = string.Empty;
        [Display(Name = "Activo")]
        public string p_activo { get; set; } = string.Empty;
        [Display(Name = "Activo Desc.")]
        public string p_activo_des { get; set; } = string.Empty;
    }
}
