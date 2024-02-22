
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;
using System.Text;

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
            //_options.Salt nunca deberia ser nula. Al tomarlo de la configuración se deberia hidratar transparentente.
#pragma warning disable CS8604 // Posible argumento de referencia nulo
            var saltOpt = Encoding.ASCII.GetBytes(_options.Salt);
#pragma warning restore CS8604 // Posible argumento de referencia nulo
                              //PBKDF2 IMPLELMETACION
            using (var alg = new PasswordDeriveBytes(password, saltOpt, "SHA256", _options.Iterations))
            {               
                var key = Convert.ToBase64String(alg.GetBytes(_options.KeySize));
#pragma warning disable CS8604 // Posible argumento de referencia nulo
                var salt = Convert.ToBase64String(alg.Salt);
#pragma warning restore CS8604 // Posible argumento de referencia nulo
                return $"{_options.Iterations}.{salt}.{key}";
            }
        }
    }
}

