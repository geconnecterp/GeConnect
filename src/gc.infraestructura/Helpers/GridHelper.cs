using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Helpers
{
    public static class GridHelper
    {
        /// <summary>
        /// Obtiene la clase de alineación CSS adecuada para un dato específico según su tipo.
        /// </summary>
        /// <param name="dato">El dato para el cual se determinará la clase de alineación. Puede ser nulo.</param>
        /// <returns>
        /// Una cadena que representa la clase de alineación CSS:
        /// - "text-sm-start" para cadenas, valores nulos u otros tipos no especificados.
        /// - "text-sm-center" para caracteres y fechas (DateTime o DateTime?).
        /// - "text-sm-end" para tipos numéricos.
        /// </returns>
        public static string ObtenerClaseAlineacion(object? dato)
        {
            if (dato == null) return "text-sm-start";

            var tipo = dato.GetType();

            if (tipo == typeof(string)) return "text-sm-start";
            if (tipo == typeof(char)) return "text-sm-center";
            if (tipo == typeof(bool)) return "text-sm-center";
            if (tipo == typeof(DateTime) || tipo == typeof(DateTime?)) return "text-sm-center";
            if (EsNumero(tipo)) return "text-sm-end";

            return "text-sm-start";
        }

        /// <summary>
        /// Formatea un dato según su tipo y un formato especificado.
        /// </summary>
        /// <param name="dato">El dato a formatear. Puede ser nulo.</param>
        /// <param name="formato">
        /// El formato a aplicar al dato. Los valores posibles incluyen:
        /// - "fecha": Formatea un DateTime como "dd/MM/yy".
        /// - "fechacompleta": Formatea un DateTime como "dd/MM/yyyy".
        /// - "hora": Formatea un DateTime como "HH:mm".
        /// - "fecha_hora": Formatea un DateTime como "dd/MM/yy HH:mm".
        /// - "bandSN": Para un carácter, devuelve "SI" si es 'S' o "NO" en caso contrario.
        /// - "monto": Para un decimal, lo formatea con dos decimales.
        /// Si no se especifica un formato válido, se utiliza un formato predeterminado.
        /// </param>
        /// <returns>
        /// Una cadena que representa el dato formateado según el tipo y el formato especificado.
        /// Si el dato es nulo, devuelve una cadena vacía.
        /// </returns>
        public static string FormatearDato(object? dato, FormatDato formato = FormatDato.Ninguno)
        {
            if (dato == null) return string.Empty;

            if (dato is DateTime fecha)
            {
                return formato switch
                {
                    FormatDato.Fecha => fecha.ToString("dd/MM/yy"),
                    FormatDato.FechaCompleta => fecha.ToString("dd/MM/yyyy"),
                    FormatDato.Hora => fecha.ToString("HH:mm"),
                    FormatDato.FechaHora => fecha.ToString("dd/MM/yy HH:mm"),
                    _ => fecha.ToString("dd/MM/yy")
                };
            }

            if (dato is char c && formato== FormatDato.BandSN)
                return c == 'S' ? "SI" : "NO";

            if (dato is int i && formato == FormatDato.Entero)
                return i.ToString("N");

            if (dato is decimal d && formato== FormatDato.Monto)
                return d.ToString("N2");

            return dato.ToString() ?? string.Empty;
        }

        public static string FormatearDato(object? dato)
        {
            return FormatearDato(dato, FormatDato.Ninguno);
        }

        private static bool EsNumero(Type tipo)
        {
            return tipo == typeof(int) || tipo == typeof(long) || tipo == typeof(float) ||
                   tipo == typeof(double) || tipo == typeof(decimal) || tipo == typeof(byte) ||
                   tipo == typeof(short) || tipo == typeof(uint) || tipo == typeof(ulong) ||
                   tipo == typeof(ushort);
        }
        public enum FormatDato
        {
            Ninguno,
            Fecha,
            FechaCompleta,
            Hora,
            FechaHora,
            BandSN,
            Monto,
            Entero
        }
    }
}
