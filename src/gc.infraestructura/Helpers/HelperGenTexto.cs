using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Helpers
{
    // ✅ NUEVO: Clase para cálculos de similitud de strings
    // ✅ AGREGAR: Clase SimilitudTexto que estaba referenciada pero no definida
    public static class SimilitudTexto
    {
        /// <summary>
        /// Calcula la distancia de Levenshtein entre dos strings
        /// </summary>
        public static int DistanciaLevenshtein(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return s2?.Length ?? 0;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            var len1 = s1.Length;
            var len2 = s2.Length;
            var matrix = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++) matrix[i, 0] = i;
            for (int j = 0; j <= len2; j++) matrix[0, j] = j;

            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[len1, len2];
        }

        public static double PorcentajeSimilitud(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 100;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

            var distancia = DistanciaLevenshtein(s1.ToLowerInvariant(), s2.ToLowerInvariant());
            var longitudMaxima = Math.Max(s1.Length, s2.Length);

            return (1.0 - (double)distancia / longitudMaxima) * 100;
        }

        public static double SimilitudJaccard(string s1, string s2, int n = 2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

            var ngramas1 = ObtenerNGramas(s1.ToLowerInvariant(), n);
            var ngramas2 = ObtenerNGramas(s2.ToLowerInvariant(), n);

            var interseccion = ngramas1.Intersect(ngramas2).Count();
            var union = ngramas1.Union(ngramas2).Count();

            return union == 0 ? 0.0 : (double)interseccion / union;
        }

        private static HashSet<string> ObtenerNGramas(string texto, int n)
        {
            var ngramas = new HashSet<string>();
            if (texto.Length < n) return ngramas;

            for (int i = 0; i <= texto.Length - n; i++)
            {
                ngramas.Add(texto.Substring(i, n));
            }
            return ngramas;
        }

        public static double SimilitudFonetica(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

            var f1 = NormalizarFonetico(s1);
            var f2 = NormalizarFonetico(s2);

            return PorcentajeSimilitud(f1, f2);
        }

        private static string NormalizarFonetico(string texto)
        {
            return texto.ToLowerInvariant()
                       .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                       .Replace("ñ", "n")
                       .Replace("c", "k").Replace("q", "k")
                       .Replace("z", "s")
                       .Replace("ph", "f")
                       .Replace("ck", "k");
        }
    }
}
