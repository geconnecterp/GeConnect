namespace gc.api.core.Constantes
{
    public static class ConstantesGC
    {
        public static class StoredProcedures
        {
            public const string FX_PASSWORD_ENCRIPTA = "dbo.sf_pass_e";
            public const string FX_PASSWORD_DESENCRIPTA = "dbo.sf_pass_d";

            public const string SP_BILLETERAORD_CARGA = "dbo.SP_BilleteraOrdenCarga";
            public const string SP_BILLETERAORD_REGISTRA = "dbo.SP_BilleteraOrdenRegistro";
            public const string SP_BILLETERAORD_NOTIFICADO = "dbo.SP_BilleteraOrdenNotificado";
            public const string SP_BILLETERAORD_VERIFICA_PAGO = "dbo.SP_BilleteraOrdenVerificaPago";
            public const string SP_BILLETERAORD_OBTENER_BY_ID = "dbo.SP_BilleteraOrdenById";

            public const string SP_ADMINISTRACION_ACTUALIZA_MEPAID = "dbo.SP_AdministracionActualizaIdMePa";
            public const string SP_CAJA_ACTUALIZA_MEPAID = "dbo.SP_CajaActualizaIdMePa";

        }

        public static class MensajeError
        {
            /// <summary>
            /// Se especifica el puntero "@campo" para que se inyecte en ese lugar el nombre del campo en conflicto.
            /// </summary>
            public const string ERR_VALOR_CAMPO_CRITICO = "No se recepcionó un campo crítico: @campo.";

            public const string ERR_AL_INSERTAR = "Hubo un error al intentar grabar en la base de datos.";

            public const string ERR_AL_ACTUALIZAR = "Hubo un error al intentar actualizar datos en la base.";
        }
    }
}
