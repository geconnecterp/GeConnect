namespace gc.api.core.Constantes
{
    public static class ConstantesGC
    {
        public static class Ordenamientos
        {
            public const string Ord_Productos = "p_id,p_desc,cta_denominacion,cta_lista,rub_id,rub_lista,p_activo,p_activo_des";
        }
        public static class StoredProcedures
        {
            public const string FX_PASSWORD_ENCRIPTA = "dbo.sf_pass_e";
            public const string FX_PASSWORD_DESENCRIPTA = "dbo.sf_pass_d";

            public const string MNU_GET_MENU_LIST = "dbo.SP_web_geco_get_menu_list";

            public const string SP_BILLETERAORD_CARGA = "dbo.SP_BilleteraOrdenCarga";
            public const string SP_BILLETERAORD_REGISTRA = "dbo.SP_BilleteraOrdenRegistro";
            public const string SP_BILLETERAORD_NOTIFICADO = "dbo.SP_BilleteraOrdenNotificado";
            public const string SP_BILLETERAORD_VERIFICA_PAGO = "dbo.SP_BilleteraOrdenVerificaPago";
            public const string SP_BILLETERAORD_OBTENER_BY_ID = "dbo.SP_BilleteraOrdenById";

            public const string SP_ADMINISTRACION_ACTUALIZA_MEPAID = "dbo.SP_AdministracionActualizaIdMePa";
            public const string SP_CAJA_ACTUALIZA_MEPAID = "dbo.SP_CajaActualizaIdMePa";
            public const string SP_PROVEEDOR_LISTA= "dbo.SPGECO_Proveedores_lista";
            public const string SP_RUBRO_LISTA = "dbo.SPGECO_Rubros_lista";
			public const string SP_PROVEEDOR_FAMILIA_LISTA = "dbo.SPGECO_Proveedores_Fiamilia_lista";

			public const string SP_PRODUCTO_BUSQUEDA = "dbo.spgeco_p_busqueda";
			public const string SP_PRODUCTO_BUSQUEDA_LISTA = "SPGECO_P_Busqueda_Lista";
            
            public const string SP_PRODUCTO_BUSQUEDA_MUCHOS = "dbo.spgeco_p_busqueda_muchos";

			public const string SP_INFOPROD_STKD = "dbo.spgeco_p_info_stk_depo";
            public const string SP_INFOPROD_STKBOX = "dbo.spgeco_p_info_stk_box";
            public const string SP_INFOPROD_STKA = "dbo.spgeco_p_info_stk_adm";
            public const string SP_INFOPROD_MOVSTK = "dbo.spgeco_p_info_mov_stk";
            public const string SP_INFOPROD_LP = "dbo.spgeco_p_info_lp";
            public const string SP_INFOPROD_TM = "dbo.SPGECO_P_Info_Tipos_Mov_Stk";
			public const string SP_INFOPROD_IE_MESES = "dbo.SPGECO_P_Info_IE_x_Mes";
			public const string SP_INFOPROD_IE_SEMANAS = "dbo.SPGECO_P_Info_IE_x_Semanas";
			public const string SP_INFOPROD_SUSTITUTO = "dbo.SPGECO_P_Info_Sustituto";
			public const string SP_INFOPROD = "dbo.SPGECO_P_Info";
			public const string SP_AJ_TIPOS = "dbo.SPGECO_AJ_Tipos";
			public const string SP_AJ_PREVIOS_CARGADOS = "dbo.SPGECO_AJ_Previos_Cargados";
			public const string SP_AJ_AJUSTE_REVERTIDO = "dbo.SPGECO_AJ_Datos";
			public const string SP_AJ_CONFIRMA = "dbo.SPGECO_AJ_Confirmar";
            public const string SP_AJ_CARGA_CONTEOS_PREVIA = "SPGECO_AJ_Cargar_Conteos_Previa";

            public const string SP_DV_PREVIOS_CARGADOS = "dbo.SPGECO_DV_Previos_Cargados";
			public const string SP_DV_DATOS = "dbo.SPGECO_DV_Datos";
			public const string SP_DV_CONFIRMA = "dbo.SPGECO_DV_Confirmar";
            public const string SP_DV_CARGA_CONTEOS_PREVIA = "SPGECO_DV_Cargar_Conteos_Previa";
            //

            public const string SP_RPR_PENDIENTES = "spgeco_RPR_Pendientes";
            public const string SP_RPR_REGISTRA = "SPGECO_RPR_Cargar_Conteos";
            public const string SP_RPR_DEPOSITOS = "SPGECO_RPR_Depositos";
            public const string SP_RPR_COMPTES_PENDIENTES = "spgeco_RPR_Comptes_Pendientes";
			public const string SP_RPR_TIPOS_COMPTES = "spgeco_RPR_Tipos_Comptes";
			public const string SP_BOX_LISTA = "SPGECO_BOX_Lista";
			public const string SP_VALIDAR_UL = "SPGECO_BOX_Valida_UL";
            public const string SP_VALIDAR_BOX = "SPGECO_BOX_Valida";
            public const string SP_BOX_ALMACENA_UL = "SPGECO_BOX_Almacena_UL";
			public const string SP_RPR_OC = "spgeco_rpr_oc";
			public const string SP_RPR_OC_D = "spgeco_rpr_oc_d";
			public const string SP_RPR_CARGAR = "spgeco_rpr_cargar";
			public const string SP_RPR_ELIMINA = "spgeco_rpr_elimina";
			public const string SP_RPR_DATOS_JSON = "spgeco_rpr_datos_json";
			public const string SP_RPR_VER_COMPTES = "spgeco_rpr_ver_comptes";
			public const string SP_RPR_VER_CONTEOS = "spgeco_rpr_ver_conteos";
			public const string SP_RPR_CONFIRMA = "spgeco_rpr_confirmar";
			public const string SP_RPR_UL = "SPGECO_UL_x_RPR";
			public const string SP_RPR_UL_D = "SPGECO_UL_d";
            
            public const string SP_CUENTA_BUSQUEDA = "dbo.spgeco_c_busqueda_lista";

            public const string SP_TR_AUTORIZACIONES_PENDIENTES = "SPGECO_TR_Pendientes";
            public const string SP_TR_Lista_BOX = "SPGECO_TR_Lista_BOX";
            public const string SP_TR_Lista_Rubros = "SPGECO_TR_Lista_Rubros";
            public const string SP_TR_Lista_Productos = "SPGECO_TR_Lista_Productos";
            public const string SP_TR_Carrito_Valida = "SPGECO_TR_Carrito_Valida";
            public const string SP_TR_Carrito_Carga = "SPGECO_TR_Carrito_Carga";
			public const string SP_TR_Pendientes = "SPGECO_TR_Pendientes";
			public const string SP_TR_Aut_Sucursales = "SPGECO_TR_Aut_Sucursales";
			public const string SP_TR_Aut_PI = "SPGECO_TR_Aut_PI";
			public const string SP_TR_Aut_PI_Detalle = "SPGECO_TR_Aut_PI_Detalle";
			public const string SP_TR_Aut_Depositos = "SPGECO_TR_Aut_Depositos";
            public const string SP_TR_Control_Salida = "SPGECO_TR_Ctl_Salida";
            public const string SP_TR_Nueva_Sin_Au = "SPGECO_TR_Nueva_Sin_Aut";
            public const string SP_TR_VALIDA_PENDIENTE = "SPGECO_TR_Valida_pendiente";
            public const string SP_TR_CONFIRMA = "SPGECO_TR_Confirmar";
            public const string SP_TR_Aut_Analiza = "SPGECO_TR_Aut_Analiza";
			public const string SP_TR_sustituto = "SPGECO_TR_Aut_Sustituto"; //En el docx figura SPGECO_TR_sustituto pero ese SP no existe
			public const string SP_TR_Aut_Nuevas = "SPGECO_TR_Aut_Nuevas";
			public const string SP_TR_VER_CTRL_SALIDA = "SPGECO_TR_Ver_CTL_Salida";
            public const string SP_TR_CARGAR_CTRL_SALIDA = "SPGECO_TR_Cargar_CTL_Salida";

            #region SP Depositos
            public const string SP_DEPOSITO_LISTA = "SPGECO_Depo_Lista";
            public const string SP_DEPOSITO_INFO_BOX = "SPGECO_DEPO_Info_BOX";
            public const string SP_DEPOSITO_INFO_STK = "SPGECO_DEPO_Info_Stk";
            public const string SP_DEPOSITO_INFO_STK_VAL = "SPGECO_DEPO_Info_Stk_Valorizado";



            #endregion

            public const string SP_TI_VALIDA_USUARIO = "SPGECO_USU_OK";
			public const string SP_TR_Ver_Conteos = "SPGECO_TR_Ver_Conteos";
			public const string SP_TR_Ctl_Salida = "SPGECO_TR_Ctl_Salida";

            public const string SP_RTR_Pendientes = "SPGECO_RTR_Pendientes";
			public const string SP_RTR_Setea_Estado = "SPGECO_RTR_Setea_Estado";
			public const string SP_RTR_Ver_Conteos = "SPGECO_RTR_Ver_Conteos";
			public const string SP_RTR_Confirma = "SPGECO_RTR_Confirmar";
			public const string SP_RTR_Verifica_Producto = "SPGECO_RTR_VERIFICA_PRODUCTO";
			public const string SP_RTR_Cargar_Conteos = "SPGECO_RTR_Cargar_Conteos";
			public const string SP_RTR_Cargar_Conteos_x_ul = "SPGECO_UL_x_RTR";

			public const string SP_OC_Productos = "SPGECO_OC_Productos";
			public const string SP_OC_Carga_Pedido = "SPGECO_OC_Carga_Pedido";//PENDIENTE (No existe en la DB)

            public const string SP_BOX_INFO = "SPGECO_BOX_Info";
            public const string SP_BOX_INFO_STK = "SPGECO_BOX_Info_Stk";
            public const string SP_BOX_INFO_MOV_STK = "SPGECO_BOX_Info_Mov_Stk";          

            #region Usuarios
            public const string SP_USU_X_IDYADM = "SPGECO_USU_X_IDYADM";
            #endregion

            #region ABM
            public const string SP_ABM_P_LISTA = "SPGECO_ABM_P_Lista";
            public const string SP_ABM_CLI_LISTA = "SPGECO_ABM_Cli_Lista";
			public const string SP_ABM_CLI_Datos = "SPGECO_ABM_Cli_Datos";
			#endregion

			#region Tipos
			public const string SP_TIPOS_NEGOCIO_LISTA = "SPGECO_Tipos_Negocios_Lista";
            public const string SP_ZONAS_LISTA = "SPGECO_Zonas_Lista";
            public const string SP_CONDICION_AFIP_LISTA = "SPGECO_Condiciones_AFIP_Lista";
            public const string SP_NATURALEZA_JURIDICA_LISTA = "SPGECO_Naturaleza_Juridicas_Lista";
            public const string SP_TIPO_DOCUMENTO_LISTA = "SPGECO_Tipos_Documentos_Lista";
            public const string SP_CONDICION_IB_LISTA = "SPGECO_Condiciones_IB_Lista";
            public const string SP_PROVINCIA_LISTA = "SPGECO_Provincias_Lista";
            public const string SP_PROVINCIA_DEPTOS_LISTA = "SPGECO_Provincias_Departamentos_Lista";
            public const string SP_TIPO_CUENTA_BCO_LISTA = "SPGECO_Tipos_Cuentas_BCO_Lista";
            public const string SP_LISTA_PRECIO = "SPGECO_LP_Lista";
            #endregion
        }
    }
}
