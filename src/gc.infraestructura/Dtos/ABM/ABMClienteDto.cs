using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.ABM
{
    public class ABMClienteSearchDto : Dto
    {
        public int total_registros { get; set; }
        public int total_paginas { get; set; }
        [Display(Name = "Id")]
        public string cta_id { get; set; } = string.Empty;
        [Display(Name = "Descripción")]
        public string cta_denominacion { get; set; } = string.Empty;
        [Display(Name = "Lista")]
        public string cta_lista { get; set; } = string.Empty;
        [Display(Name = "Domicilio")]
        public string cta_domicilio { get; set; } = string.Empty;
        [Display(Name = "Tdoc_Id")]
        public string tdoc_id { get; set; } = string.Empty;
        [Display(Name = "Tipo Doc")]
        public string tdoc_desc { get; set; } = string.Empty;
        [Display(Name = "Documento")]
        public string cta_documento { get; set; } = string.Empty;
        [Display(Name = "Habilitada")]
        public string ctac_habilitada { get; set; } = string.Empty;
        [Display(Name = "Habilitada Desc.")]
        public string ctac_habilitada_des { get; set; } = string.Empty;
    }
}
