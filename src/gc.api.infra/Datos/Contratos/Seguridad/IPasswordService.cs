using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.infra.Datos.Contratos.Security
{
    public interface IPasswordService
    {
        string Hash(string password);

        bool Check(string hash, string password);
    }
}
