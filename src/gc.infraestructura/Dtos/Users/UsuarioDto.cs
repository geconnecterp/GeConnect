using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Users
{
    public class UsuarioDto
    {
        public int Total_registros { get; set; }
        public int Total_paginas { get; set; }
        public string Usu_id { get; set; }
        public string Usu_apellidoynombre { get; set; }
        public bool Usu_bloqueado { get; set; }
        public string Tdoc_Id { get; set; }
        public string Tdoc_Desc { get; set; }
        public string? Usu_celu { get; set; }
        public string? Usu_email { get; set; }
        public string? Cta_id { get; set; }
        public string? Cta_denominacion { get; set; }
        public string? Usu_pin { get; set; }
    }
}
