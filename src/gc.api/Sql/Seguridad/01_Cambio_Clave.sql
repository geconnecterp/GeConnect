USE [geco_0000]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_USU_Clave_Cambiar
(
    @usu_id varchar(10), @clave_actual varchar(128), @clave_nueva varchar(128),
    @adm_id varchar(4) = NULL, @ip varchar(45) = NULL,
    @origen varchar(20) = 'GC.SITIO', @operacion_id uniqueidentifier = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @resultado smallint = -1,
            @resultado_id varchar(40) = 'CLAVE_CAMBIO_ERROR',
            @resultado_msj varchar(250) = 'No se pudo modificar la contraseña.',
            @resultado_setfocus varchar(40) = '',
            @ahora datetime2(0) = SYSDATETIME(),
            @guardada varchar(300), @cifrada_actual varchar(300),
            @cifrada_legacy varchar(300), @cifrada_nueva varchar(300),
            @json nvarchar(max), @longitud int,
            @val_long bit, @min smallint, @max smallint, @val_complejidad bit,
            @mayus bit, @minus bit, @numero bit, @simbolo bit, @distinta bit,
            @legacy bit, @dias smallint, @auditar bit,
            @max_intentos smallint, @ventana smallint;

    SET @usu_id = LTRIM(RTRIM(ISNULL(@usu_id, '')));
    SET @origen = LEFT(ISNULL(NULLIF(LTRIM(RTRIM(@origen)), ''), 'GC.SITIO'), 20);
    SET @operacion_id = ISNULL(@operacion_id, NEWID());

    SELECT @val_long = seg_validar_longitud, @min = seg_longitud_minima,
           @max = seg_longitud_maxima, @val_complejidad = seg_validar_complejidad,
           @mayus = seg_requiere_mayuscula, @minus = seg_requiere_minuscula,
           @numero = seg_requiere_numero, @simbolo = seg_requiere_simbolo,
           @distinta = seg_impedir_clave_actual, @legacy = seg_compatibilidad_legacy,
           @dias = seg_dias_vigencia, @auditar = seg_auditoria_activa,
           @max_intentos = seg_max_intentos_cambio,
           @ventana = seg_ventana_intentos_minutos
    FROM dbo.seguridad_configuracion WHERE seg_id = 1;

    IF @val_long IS NULL
    BEGIN
        SELECT @resultado AS resultado, 'CONFIGURACION_INEXISTENTE' AS resultado_id,
               'No se encontró la configuración de seguridad.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
        RETURN;
    END

    IF (SELECT COUNT(1) FROM dbo.usuarios_auditoria
        WHERE usu_id = @usu_id AND usa_evento = 'CLAVE_ACTUAL_INVALIDA'
          AND usa_fecha >= DATEADD(MINUTE, -@ventana, @ahora)) >= @max_intentos
    BEGIN
        SET @resultado = 9; SET @resultado_id = 'CAMBIO_TEMPORALMENTE_BLOQUEADO';
        SET @resultado_msj = 'Se alcanzó el límite de intentos. Espere unos minutos antes de volver a intentar.';
        SET @resultado_setfocus = 'ClaveActual';
        IF @auditar = 1 INSERT dbo.usuarios_auditoria
            (usu_id, usa_fecha, usa_evento, usa_resultado, usa_ip, adm_id, usa_origen, usa_detalle, usa_operacion_id)
            VALUES (@usu_id, @ahora, @resultado_id, @resultado, @ip, @adm_id, @origen, @resultado_msj, @operacion_id);
        SELECT @resultado AS resultado, @resultado_id AS resultado_id, @resultado_msj AS resultado_msj,
               @resultado_setfocus AS resultado_setfocus, @operacion_id AS OperacionId;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @guardada = usu_password FROM dbo.usuarios WITH (UPDLOCK, HOLDLOCK) WHERE usu_id = @usu_id;

        IF @guardada IS NOT NULL
        BEGIN
            SELECT @json = (SELECT @usu_id AS usuario, @clave_actual AS clave FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            SET @cifrada_actual = dbo.SF_Pass_E(@json);
            IF @legacy = 1 SET @cifrada_legacy = dbo.SF_Pass_E(@clave_actual);
        END

        IF @guardada IS NULL OR (@guardada <> @cifrada_actual AND (@legacy = 0 OR @guardada <> @cifrada_legacy))
        BEGIN
            ROLLBACK TRANSACTION;
            SET @resultado = 1; SET @resultado_id = 'CLAVE_ACTUAL_INVALIDA';
            SET @resultado_msj = 'La contraseña actual no es correcta.'; SET @resultado_setfocus = 'ClaveActual';
            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_fecha, usa_evento, usa_resultado, usa_ip, adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id, @ahora, @resultado_id, @resultado, @ip, @adm_id, @origen, @resultado_msj, @operacion_id);
            SELECT @resultado AS resultado, @resultado_id AS resultado_id, @resultado_msj AS resultado_msj,
                   @resultado_setfocus AS resultado_setfocus, @operacion_id AS OperacionId;
            RETURN;
        END

        IF @clave_nueva IS NULL OR DATALENGTH(@clave_nueva) = 0
           OR LEN(REPLACE(REPLACE(REPLACE(REPLACE(@clave_nueva, ' ', ''), CHAR(9), ''), CHAR(13), ''), CHAR(10), '')) = 0
        BEGIN SET @resultado = 2; SET @resultado_id = 'CLAVE_NUEVA_REQUERIDA'; SET @resultado_msj = 'Debe ingresar una contraseña nueva.'; END

        SET @longitud = DATALENGTH(ISNULL(@clave_nueva, ''));
        IF @resultado = -1 AND @val_long = 1 AND (@longitud < @min OR @longitud > @max)
        BEGIN SET @resultado = 3; SET @resultado_id = 'LONGITUD_INVALIDA'; SET @resultado_msj = CONCAT('La contraseña debe tener entre ', @min, ' y ', @max, ' caracteres.'); END
        IF @resultado = -1 AND @distinta = 1
           AND @clave_nueva COLLATE Latin1_General_100_BIN2 = @clave_actual COLLATE Latin1_General_100_BIN2
           AND DATALENGTH(@clave_nueva) = DATALENGTH(@clave_actual)
        BEGIN SET @resultado = 8; SET @resultado_id = 'CLAVE_REPETIDA'; SET @resultado_msj = 'La contraseña nueva debe ser diferente de la actual.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @mayus = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[A-Z]%'
        BEGIN SET @resultado = 4; SET @resultado_id = 'FALTA_MAYUSCULA'; SET @resultado_msj = 'La contraseña debe incluir al menos una letra mayúscula.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @minus = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[a-z]%'
        BEGIN SET @resultado = 5; SET @resultado_id = 'FALTA_MINUSCULA'; SET @resultado_msj = 'La contraseña debe incluir al menos una letra minúscula.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @numero = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[0-9]%'
        BEGIN SET @resultado = 6; SET @resultado_id = 'FALTA_NUMERO'; SET @resultado_msj = 'La contraseña debe incluir al menos un número.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @simbolo = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Za-z0-9 ]%'
        BEGIN SET @resultado = 7; SET @resultado_id = 'FALTA_SIMBOLO'; SET @resultado_msj = 'La contraseña debe incluir al menos un símbolo.'; END

        IF @resultado > 0
        BEGIN
            SET @resultado_setfocus = 'ClaveNueva';
            ROLLBACK TRANSACTION;
            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_fecha, usa_evento, usa_resultado, usa_ip, adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id, @ahora, 'CLAVE_POLITICA_RECHAZADA', @resultado, @ip, @adm_id, @origen, @resultado_id, @operacion_id);
            SELECT @resultado AS resultado, @resultado_id AS resultado_id, @resultado_msj AS resultado_msj,
                   @resultado_setfocus AS resultado_setfocus, @operacion_id AS OperacionId;
            RETURN;
        END

        SELECT @json = (SELECT @usu_id AS usuario, @clave_nueva AS clave FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        SET @cifrada_nueva = dbo.SF_Pass_E(@json);
        UPDATE dbo.usuarios
        SET usu_password = @cifrada_nueva, usu_expira = 1,
            usu_dias_expiracion = @dias, usu_fecha_expira_inicio = GETDATE()
        WHERE usu_id = @usu_id;

        SET @resultado = 0; SET @resultado_id = 'CLAVE_CAMBIO_OK';
        SET @resultado_msj = 'La contraseña se modificó correctamente.'; SET @resultado_setfocus = '';
        IF @auditar = 1 INSERT dbo.usuarios_auditoria
            (usu_id, usa_fecha, usa_evento, usa_resultado, usa_ip, adm_id, usa_origen, usa_detalle, usa_operacion_id)
            VALUES (@usu_id, @ahora, @resultado_id, @resultado, @ip, @adm_id, @origen, 'Cambio de contraseña confirmado.', @operacion_id);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SET @resultado = -1; SET @resultado_id = 'CLAVE_CAMBIO_ERROR';
        SET @resultado_msj = 'No se pudo modificar la contraseña.'; SET @resultado_setfocus = '';
        BEGIN TRY
            IF ISNULL(@auditar, 0) = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_fecha, usa_evento, usa_resultado, usa_ip, adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id, @ahora, @resultado_id, @resultado, @ip, @adm_id, @origen, CONCAT('Error SQL ', ERROR_NUMBER()), @operacion_id);
        END TRY BEGIN CATCH END CATCH
    END CATCH

    SELECT @resultado AS resultado, @resultado_id AS resultado_id, @resultado_msj AS resultado_msj,
           @resultado_setfocus AS resultado_setfocus, @operacion_id AS OperacionId;
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.seguridad_configuracion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_configuracion
    (
        seg_id tinyint NOT NULL,
        seg_validar_longitud bit NOT NULL CONSTRAINT DF_seg_validar_longitud DEFAULT (1),
        seg_longitud_minima smallint NOT NULL CONSTRAINT DF_seg_longitud_minima DEFAULT (8),
        seg_longitud_maxima smallint NOT NULL CONSTRAINT DF_seg_longitud_maxima DEFAULT (128),
        seg_validar_complejidad bit NOT NULL CONSTRAINT DF_seg_validar_complejidad DEFAULT (1),
        seg_requiere_mayuscula bit NOT NULL CONSTRAINT DF_seg_requiere_mayuscula DEFAULT (1),
        seg_requiere_minuscula bit NOT NULL CONSTRAINT DF_seg_requiere_minuscula DEFAULT (1),
        seg_requiere_numero bit NOT NULL CONSTRAINT DF_seg_requiere_numero DEFAULT (1),
        seg_requiere_simbolo bit NOT NULL CONSTRAINT DF_seg_requiere_simbolo DEFAULT (0),
        seg_impedir_clave_actual bit NOT NULL CONSTRAINT DF_seg_impedir_clave_actual DEFAULT (1),
        seg_compatibilidad_legacy bit NOT NULL CONSTRAINT DF_seg_compatibilidad_legacy DEFAULT (1),
        seg_dias_vigencia smallint NOT NULL CONSTRAINT DF_seg_dias_vigencia DEFAULT (30),
        seg_auditoria_activa bit NOT NULL CONSTRAINT DF_seg_auditoria_activa DEFAULT (1),
        seg_max_intentos_cambio smallint NOT NULL CONSTRAINT DF_seg_max_intentos_cambio DEFAULT (5),
        seg_ventana_intentos_minutos smallint NOT NULL CONSTRAINT DF_seg_ventana_intentos_minutos DEFAULT (15),
        seg_fecha_modificacion datetime2(0) NOT NULL CONSTRAINT DF_seg_fecha_modificacion DEFAULT (SYSDATETIME()),
        seg_usuario_modificacion varchar(10) NULL,
        CONSTRAINT PK_seguridad_configuracion PRIMARY KEY (seg_id),
        CONSTRAINT CK_seguridad_configuracion_unica CHECK (seg_id = 1),
        CONSTRAINT CK_seguridad_longitudes CHECK (seg_longitud_minima BETWEEN 1 AND 128 AND seg_longitud_maxima BETWEEN seg_longitud_minima AND 128),
        CONSTRAINT CK_seguridad_vigencia CHECK (seg_dias_vigencia BETWEEN 1 AND 3650),
        CONSTRAINT CK_seguridad_intentos CHECK (seg_max_intentos_cambio BETWEEN 1 AND 100 AND seg_ventana_intentos_minutos BETWEEN 1 AND 1440)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_configuracion WHERE seg_id = 1)
    INSERT dbo.seguridad_configuracion (seg_id) VALUES (1);
GO

IF OBJECT_ID('dbo.usuarios_auditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuarios_auditoria
    (
        usa_id bigint IDENTITY(1,1) NOT NULL,
        usu_id varchar(10) NOT NULL,
        usa_fecha datetime2(0) NOT NULL CONSTRAINT DF_usuarios_auditoria_fecha DEFAULT (SYSDATETIME()),
        usa_evento varchar(40) NOT NULL,
        usa_resultado smallint NOT NULL,
        usa_ip varchar(45) NULL,
        adm_id varchar(4) NULL,
        usa_origen varchar(20) NOT NULL CONSTRAINT DF_usuarios_auditoria_origen DEFAULT ('API'),
        usa_detalle varchar(250) NULL,
        usa_operacion_id uniqueidentifier NOT NULL,
        CONSTRAINT PK_usuarios_auditoria PRIMARY KEY (usa_id)
    );
    CREATE INDEX IX_usuarios_auditoria_usuario_fecha
        ON dbo.usuarios_auditoria (usu_id, usa_fecha DESC)
        INCLUDE (usa_evento, usa_resultado);
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SEG_Configuracion_Obtener
AS
BEGIN
    SET NOCOUNT ON;
    SELECT seg_validar_longitud AS ValidarLongitud,
           seg_longitud_minima AS LongitudMinima,
           seg_longitud_maxima AS LongitudMaxima,
           seg_validar_complejidad AS ValidarComplejidad,
           seg_requiere_mayuscula AS RequiereMayuscula,
           seg_requiere_minuscula AS RequiereMinuscula,
           seg_requiere_numero AS RequiereNumero,
           seg_requiere_simbolo AS RequiereSimbolo,
           seg_impedir_clave_actual AS ImpedirClaveActual,
           seg_dias_vigencia AS DiasVigencia
    FROM dbo.seguridad_configuracion WHERE seg_id = 1;
END
GO
