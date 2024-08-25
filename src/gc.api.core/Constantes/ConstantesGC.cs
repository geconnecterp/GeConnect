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
            public const string SP_PROVEEDOR_LISTA= "dbo.SPGECO_Proveedores_lista";
            public const string SP_RUBRO_LISTA = "dbo.SPGECO_Rubros_lista";

            public const string SP_PRODUCTO_BUSQUEDA = "dbo.spgeco_p_busqueda";

            public const string SP_INFOPROD_STKD = "dbo.spgeco_p_info_stk_depo";
            public const string SP_INFOPROD_STKBOX = "dbo.spgeco_p_info_stk_box";
            public const string SP_INFOPROD_STKA = "dbo.spgeco_p_info_stk_adm";
            public const string SP_INFOPROD_MOVSTK = "dbo.spgeco_p_info_mov_stk";
            public const string SP_INFOPROD_LP = "dbo.spgeco_p_info_lp";

            public const string SP_RPR_PENDIENTES = "spgeco_RPR_Pendientes";
            public const string SP_RPR_REGISTRA = "spgeco_RPR_Registra";
            public const string SP_RPR_DEPOSITOS = "SPGECO_RPR_Depositos";



        }

        
    }
}
