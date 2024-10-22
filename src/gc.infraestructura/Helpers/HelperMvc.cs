using gc.infraestructura.Dtos.Almacen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Helpers
{
    public class HelperMvc<T> where T : class
    {
        public static SelectList ListaGenerica(IEnumerable<T> listado,string textoValor,string textoDato,object valorSeleccionado = null)
        {
            if (valorSeleccionado != null)
            {
                return new SelectList(listado, textoValor, textoDato, valorSeleccionado);
            }
            else
            {
                return new SelectList(listado, textoValor, textoDato);
            }
        }

        public static SelectList ListaGenerica(IEnumerable<T> listado, object valorSeleccionado = null)
        {
            return ListaGenerica(listado, "Id", "Descripcion", valorSeleccionado);
        }

        public static SelectList ListaGenerica(IEnumerable<T> listado)
        {
            return ListaGenerica(listado, "Id", "Descripcion");
        }
	}
}
