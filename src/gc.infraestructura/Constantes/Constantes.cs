namespace gc.infraestructura.Constantes
{
    public class Constantes
    {
        public static class MensajeError
        {
            /// <summary>
            /// Se especifica el puntero "@campo" para que se inyecte en ese lugar el nombre del campo en conflicto.
            /// </summary>
            public const string ERR_VALOR_CAMPO_CRITICO = "No se recepcionó un campo crítico: @campo.";

            public const string ERR_AL_INSERTAR = "Hubo un error al intentar grabar en la base de datos.";

            public const string ERR_AL_ACTUALIZAR = "Hubo un error al intentar actualizar datos en la base.";

            public const string RPR_PRODUCTO_NO_ENCONTRADO_ACUMULACION = "No se encontró el producto en la lista de productos ingresados. Verifique. Si el problema persiste informa al administardor del sistema.";
            public const string RPR_PRODUCTO_ACUMULACION_UNIDAD_BULTO_DISTINTO = "La cantidad de unidades en el bulto difieren del ya cargado. Verifique. ";
        }

        public static class MensajesOK
        {
            public const string RPR_PRODUCTO_ACUMULADO = "Se acumularon las cantidades del producto exitosamente.";
            public const string RPR_PRODUCTO_REMPLAZADO = "Se remplazo el producto exitosamente.";

        }
    }
}
