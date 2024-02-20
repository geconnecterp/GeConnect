
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;

namespace gc.api.infra.Datos.Implementacion.Security
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordOptions _options;

        public PasswordService(IOptions<PasswordOptions> options)
        {
            _options = options.Value;
        }

        public bool Check(string hash, string password)
        {
            var parts = hash.Split('.');
            if (parts.Length != 3)
            {
                throw new FormatException("Expected Hash Format");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var alg = new PasswordDeriveBytes(password, salt, "SHA256", iterations))
            {
                var keyToCheck = alg.GetBytes(_options.KeySize);

                return keyToCheck.SequenceEqual(key);
            }
        }

        public string Hash(string password)
        {           
            //PBKDF2 IMPLELMETACION
            using (var alg = new PasswordDeriveBytes(password, null, "SHA256", _options.Iterations))
            {
                if (alg.Salt != null)
                {
                    var key = Convert.ToBase64String(alg.GetBytes(_options.KeySize));
                    var salt = Convert.ToBase64String(alg.Salt);
                    return $"{_options.Iterations}.{salt}.{key}";
                }
                return "";
            }
        }
    }
}

