using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.infra.Datos.Contratos.Security
{
    public interface IPasswordService
    {
        string CalculaClave(RegistroUserDto registroUserDto);

        bool Check(string hash,string usuario, string password);
    }
}
