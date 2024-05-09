namespace gc.api.core.Constantes
{
    public static class ConstantesGC
    {
        public static class StoredProcedures
        {
            public const string FX_PASSWORD_ENCRIPTA = "dbo.sf_pass_e";
            public const string FX_PASSWORD_DESENCRIPTA = "dbo.sf_pass_d";

            public const string SP_BILLETERA_CARGA = "SP_BilleteraOrdenCarga";
            public const string SP_BILLETERA_REGISTRA = "SP_BilleteraOrdenRegistra";
            public const string SP_BILLETERA_NOTIRICADO = "SP_BilleteraOrdenNotificado";

        }

        public static class MensajeError
        {
            /// <summary>
            /// Se especifica el puntero "@campo" para que se inyecte en ese lugar el nombre del campo en conflicto.
            /// </summary>
            public const string ERR_VALOR_CAMPO_CRITICO = "No se recepcionó un campo crítico: @campo";

            public const string ERR_AL_INSERTAR = "Hubo un error al intentar grabar en la base de datos";
        }
    }
}
