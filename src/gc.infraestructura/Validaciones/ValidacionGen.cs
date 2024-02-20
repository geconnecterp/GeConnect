using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace gc.infraestructura.Core.Validaciones
{
    public class ValidacionGen
    {
        public static bool ValidarEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");
        }
    }
}
