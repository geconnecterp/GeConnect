using gc.infraestructura.Core.Enumeraciones;
using System;

namespace gc.infraestructura.Core.Validaciones
{
    public class ValidacionCuil
    {
        #region Validacion CUIL
        public static ValidezCuilCuit ValidarCuilCuit(string cuilCuit)
        {
            if (PoseeEspaciosONull(cuilCuit))
                return ValidezCuilCuit.CuilInvalido;

            if (cuilCuit.Length != 11)
                return ValidezCuilCuit.LongitudDistintaDe11;

            if (!EsNumerico(cuilCuit))
                return ValidezCuilCuit.PoseeElementosNoNumericos;

            if (!cuilCuit.Substring(0, 2).Equals("20") &&
                !cuilCuit.Substring(0, 2).Equals("23") &&
                !cuilCuit.Substring(0, 2).Equals("24") &&
                !cuilCuit.Substring(0, 2).Equals("27") &&
                !cuilCuit.Substring(0, 2).Equals("30"))
                return ValidezCuilCuit.CuilInvalido;

            if (!VerificarCuil(cuilCuit))
                return ValidezCuilCuit.CuilInvalido;

            return ValidezCuilCuit.CuilValido;
        }

        private static bool VerificarCuil(string cuilCuit)
        {
            int[] digitos = new int[11];

            for (int i = 0; i < cuilCuit.Length; i++)
            {
                digitos[i] = Convert.ToInt32(cuilCuit.Substring(i, 1));
            }

            int suma, resto, verif = 0;

            for (int i = 0; i < digitos.Length; i++)
            {
                suma = digitos[0] * 5 +
                       digitos[1] * 4 +
                       digitos[2] * 3 +
                       digitos[3] * 2 +
                       digitos[4] * 7 +
                       digitos[5] * 6 +
                       digitos[6] * 5 +
                       digitos[7] * 4 +
                       digitos[8] * 3 +
                       digitos[9] * 2;

                resto = suma % 11;

                if (resto == 0)
                {
                    verif = 0;
                    break;
                }
                else
                    if (resto == 1 && (digitos[1] == 0 || digitos[6] == 7))
                {
                    digitos[1] = 4;
                    continue;
                }
                else
                {
                    verif = 11 - resto;
                    break;
                }
            }

            return digitos[10] == verif;
        }

        #endregion

        #region Validacion Documento

        public static ValidezDocumento ValidarDocumento(string documento)
        {
            if (PoseeEspaciosONull(documento))
                return ValidezDocumento.DocumentoInvalido;

            if (!EsNumerico(documento))
                return ValidezDocumento.PoseeElementosNoNumericos;

            if (!NoEsCeroNi99999999(documento))
                return ValidezDocumento.DocumentoIgualACeroO99999999;

            return ValidezDocumento.DocumentoValido;
        }
        #endregion

        #region Métodos Privados
        private static bool NoEsCeroNi99999999(string docCuil)
        {
            long num = long.Parse(docCuil);
            if (num == 0 || num == 99999999)
                return false;

            return true;
        }

        private static bool EsNumerico(string docCuil)
        {
            try
            {
                long c = long.Parse(docCuil);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool PoseeEspaciosONull(string docCuil)
        {
            if (string.IsNullOrEmpty(docCuil))
                return true;
            return false;
        }

        #endregion

        #region Métodos Públicos
        public static bool EsNumeroMenorOIgualACero(int numero)
        {
            if (numero <= 0)
                return true;
            return false;
        }
        public static bool EsNumeroMenorOIgualACero(short numero)
        {
            if (numero <= 0)
                return true;
            return false;
        }
        public static bool EsNumeroMenorOIgualACero(long numero)
        {
            if (numero <= 0)
                return true;
            return false;
        }
        public static bool EsNumeroMenorOIgualACero(decimal numero)
        {
            if (numero <= 0)
                return true;
            return false;
        }

        #endregion
    }
}
