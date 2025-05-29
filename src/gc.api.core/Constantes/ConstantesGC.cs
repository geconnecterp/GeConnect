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
			public const string SP_PROVEEDOR_FAMILIA_LISTA = "dbo.SPGECO_Proveedores_Familia_lista";
            public const string SP_UP_LISTA = "SPGECO_UP_Lista";
            public const string SP_BARRADO_LISTA = "SPGECO_ABM_P_Barrados_Lista";
            public const string SP_BARRADO_DATO = "SPGECO_ABM_P_Barrados_Datos";
            
            public const string SP_LIMITESTK_LISTA = "SPGECO_ABM_P_LimiteStk_Lista";
            public const string SP_LIMITESTK_DATO = "SPGECO_ABM_P_LimiteStk_Datos";



            public const string SP_PRODUCTO_BUSQUEDA = "dbo.spgeco_p_busqueda";
			public const string SP_PRODUCTO_BUSQUEDA_LISTA = "SPGECO_P_Busqueda_Lista";
			public const string SP_IVA_SITUACION_LISTA = "SPGECO_IVA_Situacion_Lista";            
			public const string SP_IVA_ALICUOTA_LISTA = "SPGECO_IVA_Alicuotas_Lista";

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
            public const string SP_INFO_UL_REPO = "SPGECO_UL_repo";


            public const string SP_CUENTA_BUSQUEDA = "dbo.spgeco_c_busqueda_lista";
            public const string SP_CUENTA_DATO = "SPGECO_C_Contacto_Datos";

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

			#region Orden de Compra
			public const string SP_OC_Productos = "SPGECO_OC_Productos";
			public const string SP_OC_Pendientes = "SPGECO_OC_Pendientes";
			public const string SP_OC_Carga_Pedido = "SPGECO_OC_Carga_Pedido";
			public const string SP_OC_Carga_Detalle = "SPGECO_OC_Carga_Detalle";
			public const string SP_OC_Tope = "SPGECO_OC_Tope";
			public const string SP_OC_Carga_Resumen = "SPGECO_OC_Carga_Resumen";
			public const string SP_OC_Confirmar = "SPGECO_OC_Confirma";
			public const string SP_OC_Lista = "SPGECO_OC_Lista";
			public const string SP_OC_Detalle = "SPGECO_OC_d";
			public const string SP_OC_RPR_ASOCIADA = "SPGECO_OC_RPR_Asociadas";
			public const string SP_OC_OBTENER_POR_OC_COMPTE = "SPGECO_OC_Obtener_Por_Occompte";

			//Acciones sobre las OC
			public const string SP_OC_ACCIONES_ACTIVAR = "SPGECO_OC_Activar";
			public const string SP_OC_ACCIONES_CERRAR = "SPGECO_OC_Cerrar";
			public const string SP_OC_ACCIONES_ANULAR = "SPGECO_OC_Anular";
			public const string SP_OC_ACCIONES_DESANULAR = "SPGECO_OC_DesAnular";
			public const string SP_OC_ACCIONES_CAMBIA_ADM = "SPGECO_OC_Cambia_Adm";
			public const string SP_OC_VALIDAR = "SPGECO_OC_Validar";
			#endregion

			public const string SP_BOX_INFO = "SPGECO_BOX_Info";
            public const string SP_BOX_INFO_STK = "SPGECO_BOX_Info_Stk";
            public const string SP_BOX_INFO_MOV_STK = "SPGECO_BOX_Info_Mov_Stk";

			#region Orden de Pago a Proveedores
			public const string SP_OP_VALIDACIONES_PREV = "SPGECO_OP_Validaciones_Prev";
            public const string SP_OP_VTO = "SPGECO_OP_Vto";
            public const string SP_OP_CARGAR_SACAR = "SPGECO_OP_cargar_sacar";
			public const string SP_OP_RETENCIONES = "SPGECO_OP_Retenciones";
			public const string SP_OP_VALORES = "SPGECO_OP_Valores";
            public const string SP_OP_SV_TCF = "SPGECO_OP_SV_tcf";
			public const string SP_OP_SV_CTAF = "SPGECO_OP_SV_Ctaf";
			#endregion

			#region Cliente
			public const string SP_CLIENTE_LISTA = "spgeco_clientes_lista";
            #endregion

            #region Usuarios
            public const string SP_USU_X_IDYADM = "SPGECO_USU_X_IDYADM";
            public const string SP_USU_PERFILES_LISTA = "SPGECO_ABM_Usu_perfiles_Lista";
            public const string SP_USU_PERFIL_DATOS = "SPGECO_ABM_Usu_perfiles_Datos";
            public const string SP_USU_PERFxUSU_LISTA = "SPGECO_ABM_Usu_Perfiles_Usu_Lista";
            /// <summary>
            /// SE OBTIENEN TODOS LOS MENUES QUE EXISTEN EN LA BASE
            /// </summary>
            public const string SP_MENU_ID = "SPGECO_ABM_Usu_Menues";
            /// <summary>
            /// SE OBTIENE TODO EL MENU CON LAS MARCAS QUE CORRESPONDEN A UN PERFIL ESPECIFICO
            /// </summary>
            public const string SP_MENU_ITEMS = "SPGECO_ABM_Usu_Menu_x_Perfiles";
            /// <summary>
            /// SE OBTIENEN TODOS LOS PERFILES DE UN USUARIO
            /// </summary>
            public const string SP_USU_PERFILES = "SPGECO_Usu_Perfiles_x_Usu";            
            /// <summary>
            /// Este SP hace predeterminado un perfil de usuario, para un usuario especifico.
            /// </summary>
            public const string SP_USU_PERFIL_DEFAULT = "SPGECO_Usu_Perfil_Usu_Default_update";
            /// <summary>
            /// SE OBTIENE EL MENU ESPECIFICO DEL USUARIO, SEGUN SU PERFIL         
            /// </summary>
            public const string SP_USU_MENU_PERSONAL = "SPGECO_Usu_Menu_x_Perfiles";
            /// <summary>
            /// SE DEVUELVE UNA LISTA DE USUARIOS EN FUNCION DEL FILTRO DE DATOS
            /// </summary>
            public const string SP_USU_FILTRO = "SPGECO_ABM_Usu_Usu_Lista";
            /// <summary>
            /// SE DEVUELVE LOS DATOS DEL USUARIO
            /// </summary>
            public const string SP_USU_DATO = "SPGECO_ABM_Usu_Usu_Datos";
            /// <summary>
            /// DEVUELVE TODOS LOS PERFILES EXISTENTES PERO SE DETALLA CUALES ESTAN ASIGNADOS Y CUAL ES EL DEFAULT.
            /// </summary>
            public const string SP_USU_PERFIL = "SPGECO_ABM_Usu_USU_PERFIL";
            /// <summary>
            /// DEVUELVE TODAS LAS ADMINISTRACIONES CON LA MARCA EN CUALES ESTA ASIGNADO.
            /// </summary>
            public const string SP_USU_ADM = "SPGECO_ABM_USU_USU_ADM";
            /// <summary>
            /// DEVUELVE TODOS LOS DERECHOS CON EL DETALLE DE LOS DERECHOS ASIGNADOS.
            /// </summary>
            public const string SP_USU_DER = "SPGECO_ABM_USU_USU_DER";

            #endregion

            #region Consultas de Cuenta Corriente

            public const string SP_CONS_CTACTE= "SPGECO_C_CtaCte";
            public const string SP_CONS_VENCIMIENTOS_CMP_SINPUTAR = "SPGECO_C_Vto";
            public const string SP_CONS_COMPROBANTES_TOT = "SPGECO_C_Comptes_Tot";
            public const string SP_CONS_COMPROBANTES_DET = "SPGECO_C_Comptes_Det";
            public const string SP_CONS_OPAGO_PROVEEDORES = "SPGECO_C_OP";
            public const string SP_CONS_OPAGO_PROVEEDORES_DET = "SPGECO_C_OP_d";
            public const string SP_CONS_RECEPCIONES_PROV = "SPGECO_C_RP";
            public const string SP_CONS_RECEPCIONES_PROV_DET = "SPGECO_C_RP_d";

            #endregion

            #region ABM
            public const string SP_ABM_P_LISTA = "SPGECO_ABM_P_Lista";
            public const string SP_ABM_P_DATOS = "SPGECO_ABM_P_Datos";
            
            public const string SP_ABM_CLI_LISTA = "SPGECO_ABM_Cli_Lista";
			public const string SP_ABM_CLI_Datos = "SPGECO_ABM_Cli_Datos";
			public const string SP_ABM_CLI_FP_Lista = "SPGECO_ABM_Cuenta_FP_Lista";
			public const string SP_ABM_CLI_FP_Datos = "SPGECO_ABM_Cuenta_FP_Datos";
            //
            public const string SP_ABM_CLI_CONTACTOS_Lista = "SPGECO_ABM_Cuenta_CONTACTOS_Lista";
            public const string SP_ABM_CLI_CONTACTOS_Datos = "SPGECO_ABM_Cuenta_CONTACTOS_Datos";
            public const string SP_ABM_CLI_OBS_Lista = "SPGECO_ABM_Cuenta_Obs_Lista";
			public const string SP_ABM_CLI_OBS_Datos = "SPGECO_ABM_Cuenta_Obs_Datos";
			public const string SP_ABM_CLI_NOTA_Lista = "SPGECO_ABM_Cuenta_Nota_Lista";
			public const string SP_ABM_CLI_NOTA_Datos = "SPGECO_ABM_Cuenta_Nota_Datos";

			public const string SP_ABM_PROV_LISTA = "SPGECO_ABM_Prov_Lista";
			public const string SP_ABM_PROV_DATOS = "SPGECO_ABM_Prov_Datos";
			public const string SP_ABM_PROV_FAMILIA_LISTA = "SPGECO_ABM_Prov_Familia_Lista";
			public const string SP_ABM_PROV_FAMILIA_DATOS = "SPGECO_ABM_Prov_Familia_Datos";

			public const string SP_ABM_CONFIRMAR = "SPGECO_ABM_Confirmar";

            public const string SP_ABM_SECTOR_LISTA = "SPGECO_ABM_Sectores_Lista";
            public const string SP_ABM_SECTOR_DATOS = "SPGECO_ABM_Sectores_Datos";
			public const string SP_ABM_SUB_SECTOR_LISTA = "SPGECO_ABM_Sub_Sectores_Lista";
			public const string SP_ABM_SUB_SECTOR_DATOS = "SPGECO_ABM_Sub_Sectores_Datos";
			public const string SP_ABM_RUBRO_LISTA = "SPGECO_ABM_Rubros_Lista";
			public const string SP_ABM_RUBRO_DATOS = "SPGECO_ABM_Rubros_Datos";

            public const string SP_ABM_MEDIOS_PAGOS_LISTA = "SPGECO_ABM_MediosPagos_Lista";
			public const string SP_ABM_MEDIOS_PAGOS_DATOS = "SPGECO_ABM_MediosPagos_Datos";
			public const string SP_ABM_OPCION_CUOTA_LISTA = "SPGECO_ABM_MediosPagos_Cuotas_Lista";
			public const string SP_ABM_OPCION_CUOTA_DATOS = "SPGECO_ABM_MediosPagos_Cuotas_Datos";
			public const string SP_ABM_CUENTA_FIN_LISTA = "SPGECO_ABM_MediosPagos_Ctaf_Lista";
			public const string SP_ABM_CUENTA_FIN_DATOS = "SPGECO_ABM_MediosPagos_Ctaf_Datos";
			public const string SP_ABM_BANCO_LISTA = "SPGECO_ABM_Bancos_Lista";
			public const string SP_ABM_BANCO_DATOS = "SPGECO_ABM_Bancos_Datos";
			public const string SP_ABM_GASTOS_LISTA = "SPGECO_ABM_Gastos_Lista";
			public const string SP_ABM_GASTOS_DATOS = "SPGECO_ABM_Gastos_Datos";

            public const string SP_ABM_VENDEDOR_LISTA = "SPGECO_ABM_Ve_Lista";
            public const string SP_ABM_VENDEDOR_DATO = "SPGECO_ABM_Ve_Datos";

            public const string SP_ABM_REPARTIDOR_LISTA = "SPGECO_ABM_RT_Lista";
            public const string SP_ABM_REPARTIDOR_DATO = "SPGECO_ABM_RT_Datos";

            public const string SP_ABM_ZONA_LISTA = "SPGECO_ABM_Zn_Lista";
            public const string SP_ABM_ZONA_DATO = "SPGECO_ABM_Zn_Datos";

            public const string SP_CCB_PLANCUENTAS_LISTA = "SPGECO_ABM_CCB_Lista";
            public const string SP_CCB_PLANCUENTAS_DATO = "SPGECO_ABM_CCB_Datos";

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
            public const string SP_TIPO_CANAL_LISTA = "SPGECO_Tipos_Canales_Lista";
            public const string SP_VENDEDOR_LISTA = "SPGECO_Vendedores_Lista";
            public const string SP_REPARTIDOR_LISTA = "SPGECO_Repartidores_Lista";
            public const string SP_FINANCIEROS_LISTA = "SPGECO_Financieros_Lista";
			public const string SP_FINANCIEROS_RELA_LISTA = "SPGECO_Financieros_Rela_Lista";
			public const string SP_FORMA_PAGO_LISTA = "SPGECO_Formas_Pagos_Lista";
			public const string SP_TIPO_CONTACTO_LISTA = "SPGECO_Tipos_Contactos_Lista";
            public const string SP_TIPO_OBS_LISTA = "SPGECO_Tipos_Obs_Lista";
            public const string SP_TIPO_OPE_IVA = "SPGECO_Tipos_Ope_IVA";
            public const string SP_TIPO_PROV = "SPGECO_Tipos_Prov";
			public const string SP_TIPO_GASTO = "SPGECO_G_Cuentas_Lista";
			public const string SP_TIPO_RET_GAN = "SPGECO_Reten_GA";
			public const string SP_TIPO_RET_IB = "SPGECO_Reten_IB";
			public const string SP_TIPO_CUENTA_FIN_LISTA = "SPGECO_Tipos_Cuentas_Fin_Lista";
			public const string SP_TIPOS_MONEDA_LISTA = "SPGECO_Monedas";
			public const string SP_FINANCIERO_ESTADOS = "SPGECO_Financieros_Estados";
			public const string SP_CCB_CUENTA_LISTA = "SPGECO_CCB_Cuentas_Lista";
			public const string SP_TIPO_CUENTA_GASTO_LISTA = "SPGECO_Tipos_Cuentas_Gastos_Lista";
			public const string SP_OC_ESTADO_LISTA = "SPGECO_Estados_OC_Lista";
			public const string SP_TIPO_COMPROBANTE = "SPGECO_Tipos_Comprobantes";
            public const string SP_TIPO_TRIBUTO = "SPGECO_Tipos_Tributos";
			public const string SP_TIPO_DTOS_VALORIZA_RPR_LISTA = "SPGECO_Tipos_Dtos_Valoriza_RPR_Lista";
			//
			#endregion

			public const string SP_ADMINISTRACIONES = "SPGECO_Administraciones";
			public const string SP_COMPTE_DATOS_PROV = "SPGECO_Compte_DatosProv";
			public const string SP_COMPTE_CARGA_RPR = "SPGECO_Compte_Carga_RPR";
			public const string SP_COMPTE_CARGA_A_CTA = "SPGECO_Compte_Carga_A_Cta";
			public const string SP_COMPTE_CARGA_CONFIRMA = "SPGECO_Compte_Carga_Confirma";
			public const string SP_COMPTE_VALORIZA_PENDIENTES = "SPGECO_Compte_Valoriza_Pendientes";
			public const string SP_COMPTE_VALORIZA_DETALLE_RPR = "SPGECO_Compte_Valoriza_Detalle_RPR";
			public const string SP_COMPTE_VALORIZA_DTOS = "SPGECO_Compte_Valoriza_Dtos";
			public const string SP_COMPTE_VALORIZA = "SPGECO_Compte_Valoriza";
			public const string SP_COMPTE_VALORIZA_COSTO_OC = "SPGECO_Compte_Valoriza_Costo_OC";

            

            #region Asientos
            public const string SP_EJERCICIOS_LISTA = "SPGECO_Conta_Ejercicios";
            public const string SP_CONTA_USU_ASIENTOS = "SPGECO_Conta_Usu_Asientos";
            public const string SP_TIPO_ASIENTO = "SPGECO_Tipos_Asientos";
            public const string SP_CONTA_ASIENTO_TMP = "SPGECO_Conta_Asiento_TMP_Lista";
            public const string SP_ASIENTO_TMP_PASA = "SPGECO_Conta_Asiento_TMP_Pasa";
            public const string SP_ASIENTO_TMP_DETALLE = "SPGECO_Conta_Asiento_TMP_Datos";
            #endregion
        }
    }
    
}
