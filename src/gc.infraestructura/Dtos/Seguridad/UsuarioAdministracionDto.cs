using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos.Seguridad
{
    public partial class UsuarioAdministracionDto:Dto
    {
        public UsuarioAdministracionDto()
        {
            Usu_Id = string.Empty;
            Adm_Id = string.Empty;
        }
        public string Usu_Id { get; set; }
        public string Adm_Id { get; set; }

    }
}
