USE [geco_0000]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    Gestión segura de credenciales administrativas.
    Script idempotente: puede ejecutarse nuevamente sin recrear datos existentes.
    No asigna códigos de derecho ni contiene la contraseña temporal.
*/

IF COL_LENGTH('dbo.usuarios', 'usu_cambio_clave_obligatorio') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_cambio_clave_obligatorio bit NOT NULL
        CONSTRAINT DF_usuarios_cambio_clave_obligatorio DEFAULT (0) WITH VALUES;
GO
IF COL_LENGTH('dbo.usuarios', 'usu_cambio_clave_motivo') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_cambio_clave_motivo varchar(20) NULL;
GO
IF COL_LENGTH('dbo.usuarios', 'usu_cambio_clave_fecha') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_cambio_clave_fecha datetime2(0) NULL;
GO
IF COL_LENGTH('dbo.usuarios', 'usu_cambio_clave_vencimiento') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_cambio_clave_vencimiento datetime2(0) NULL;
GO
IF COL_LENGTH('dbo.usuarios', 'usu_cambio_clave_operacion_id') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_cambio_clave_operacion_id uniqueidentifier NULL;
GO
IF COL_LENGTH('dbo.usuarios', 'usu_version_credencial') IS NULL
    ALTER TABLE dbo.usuarios ADD usu_version_credencial int NOT NULL
        CONSTRAINT DF_usuarios_version_credencial DEFAULT (0) WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_usuarios_cambio_clave_estado')
    ALTER TABLE dbo.usuarios WITH CHECK ADD CONSTRAINT CK_usuarios_cambio_clave_estado CHECK
    (
        usu_cambio_clave_obligatorio = 0 OR
        (usu_cambio_clave_motivo IS NOT NULL AND usu_cambio_clave_fecha IS NOT NULL
         AND usu_cambio_clave_operacion_id IS NOT NULL)
    );
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_usuarios_cambio_clave_vencimiento')
    ALTER TABLE dbo.usuarios WITH CHECK ADD CONSTRAINT CK_usuarios_cambio_clave_vencimiento CHECK
    (
        usu_cambio_clave_vencimiento IS NULL OR usu_cambio_clave_fecha IS NULL OR
        usu_cambio_clave_vencimiento > usu_cambio_clave_fecha
    );
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_usuarios_version_credencial')
    ALTER TABLE dbo.usuarios WITH CHECK ADD CONSTRAINT CK_usuarios_version_credencial
        CHECK (usu_version_credencial >= 0);
GO

IF COL_LENGTH('dbo.usuarios_auditoria', 'usa_usu_ejecutor') IS NULL
    ALTER TABLE dbo.usuarios_auditoria ADD usa_usu_ejecutor varchar(10) NULL;
GO

IF COL_LENGTH('dbo.seguridad_configuracion', 'seg_clave_temporal_vigencia_horas') IS NULL
    ALTER TABLE dbo.seguridad_configuracion ADD seg_clave_temporal_vigencia_horas smallint NOT NULL
        CONSTRAINT DF_seg_clave_temporal_vigencia_horas DEFAULT (24) WITH VALUES;
GO
IF COL_LENGTH('dbo.seguridad_configuracion', 'seg_derecho_blanquear_clave') IS NULL
    ALTER TABLE dbo.seguridad_configuracion ADD seg_derecho_blanquear_clave varchar(10) NULL;
GO
IF COL_LENGTH('dbo.seguridad_configuracion', 'seg_derecho_desbloquear_usuario') IS NULL
    ALTER TABLE dbo.seguridad_configuracion ADD seg_derecho_desbloquear_usuario varchar(10) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_seg_clave_temporal_vigencia')
    ALTER TABLE dbo.seguridad_configuracion WITH CHECK ADD CONSTRAINT CK_seg_clave_temporal_vigencia
        CHECK (seg_clave_temporal_vigencia_horas BETWEEN 1 AND 720);
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
           seg_dias_vigencia AS DiasVigencia,
           seg_clave_temporal_vigencia_horas AS ClaveTemporalVigenciaHoras,
           seg_derecho_blanquear_clave AS DerechoBlanquearClave,
           seg_derecho_desbloquear_usuario AS DerechoDesbloquearUsuario
    FROM dbo.seguridad_configuracion WHERE seg_id = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_USU_Seguridad_Estado
    @usu_id varchar(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT usu_cambio_clave_obligatorio AS CambioClaveObligatorio,
           usu_cambio_clave_motivo AS CambioClaveMotivo,
           usu_cambio_clave_fecha AS CambioClaveFecha,
           usu_cambio_clave_vencimiento AS CambioClaveVencimiento,
           usu_cambio_clave_operacion_id AS CambioClaveOperacionId,
           usu_version_credencial AS VersionCredencial,
           CASE WHEN usu_cambio_clave_obligatorio = 1
                     AND usu_cambio_clave_vencimiento IS NOT NULL
                     AND SYSDATETIME() >= usu_cambio_clave_vencimiento
                THEN CONVERT(bit, 1) ELSE CONVERT(bit, 0) END AS ClaveTemporalVencida
    FROM dbo.usuarios
    WHERE usu_id = LTRIM(RTRIM(@usu_id));
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_USU_Clave_Blanquear
(
    @usu_id_objetivo varchar(10), @usu_id_ejecutor varchar(10),
    @clave_temporal varchar(128), @adm_id varchar(4) = NULL,
    @ip varchar(45) = NULL, @origen varchar(20) = 'GC.SITIO',
    @operacion_id uniqueidentifier = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @resultado smallint = -1, @resultado_id varchar(40) = 'BLANQUEO_ERROR',
            @resultado_msj varchar(250) = 'No se pudo blanquear la contraseña.',
            @ahora datetime2(0) = SYSDATETIME(), @horas smallint, @auditar bit,
            @json nvarchar(max), @cifrada varchar(300), @dias smallint;

    SET @usu_id_objetivo = LTRIM(RTRIM(ISNULL(@usu_id_objetivo, '')));
    SET @usu_id_ejecutor = LTRIM(RTRIM(ISNULL(@usu_id_ejecutor, '')));
    SET @origen = LEFT(ISNULL(NULLIF(LTRIM(RTRIM(@origen)), ''), 'GC.SITIO'), 20);
    SET @operacion_id = ISNULL(@operacion_id, NEWID());

    SELECT @horas = seg_clave_temporal_vigencia_horas,
           @auditar = seg_auditoria_activa, @dias = seg_dias_vigencia
    FROM dbo.seguridad_configuracion WHERE seg_id = 1;

    IF @horas IS NULL
        SELECT @resultado AS resultado, 'CONFIGURACION_INEXISTENTE' AS resultado_id,
               'No se encontró la configuración de seguridad.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
    ELSE IF @usu_id_objetivo = '' OR NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE usu_id = @usu_id_objetivo)
        SELECT 1 AS resultado, 'USUARIO_INEXISTENTE' AS resultado_id,
               'El usuario seleccionado no existe.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
    ELSE IF @usu_id_ejecutor = '' OR NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE usu_id = @usu_id_ejecutor)
        SELECT 2 AS resultado, 'EJECUTOR_INVALIDO' AS resultado_id,
               'No fue posible identificar al usuario que realiza la operación.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
    ELSE IF @usu_id_objetivo = @usu_id_ejecutor
        SELECT 3 AS resultado, 'AUTOBLANQUEO_NO_PERMITIDO' AS resultado_id,
               'No puede blanquear su propia contraseña.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
    ELSE IF @clave_temporal IS NULL OR DATALENGTH(@clave_temporal) = 0 OR DATALENGTH(@clave_temporal) > 128
        SELECT 4 AS resultado, 'CLAVE_TEMPORAL_INVALIDA' AS resultado_id,
               'La configuración de la contraseña temporal no es válida.' AS resultado_msj,
               '' AS resultado_setfocus, @operacion_id AS OperacionId;
    ELSE
    BEGIN
        BEGIN TRY
            SELECT @json = (SELECT @usu_id_objetivo AS usuario, @clave_temporal AS clave
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            SET @cifrada = dbo.SF_Pass_E(@json);

            BEGIN TRANSACTION;
            UPDATE dbo.usuarios WITH (UPDLOCK)
               SET usu_password = @cifrada,
                   usu_cambio_clave_obligatorio = 1,
                   usu_cambio_clave_motivo = 'BLANQUEO',
                   usu_cambio_clave_fecha = @ahora,
                   usu_cambio_clave_vencimiento = DATEADD(HOUR, @horas, @ahora),
                   usu_cambio_clave_operacion_id = @operacion_id,
                   usu_version_credencial = usu_version_credencial + 1,
                   usu_expira = 1,
                   usu_dias_expiracion = @dias,
                   usu_fecha_expira_inicio = @ahora
             WHERE usu_id = @usu_id_objetivo;

            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_usu_ejecutor, usa_fecha, usa_evento, usa_resultado, usa_ip,
                 adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id_objetivo, @usu_id_ejecutor, @ahora, 'CLAVE_BLANQUEADA', 0, @ip,
                        @adm_id, @origen, CONCAT('Cambio obligatorio activado por ', @horas, ' horas.'), @operacion_id);
            COMMIT TRANSACTION;

            SELECT 0 AS resultado, 'CLAVE_BLANQUEADA' AS resultado_id,
                   'La contraseña fue blanqueada. El usuario deberá reemplazarla en el próximo ingreso.' AS resultado_msj,
                   '' AS resultado_setfocus, @operacion_id AS OperacionId;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT -1 AS resultado, 'BLANQUEO_ERROR' AS resultado_id,
                   'No se pudo blanquear la contraseña.' AS resultado_msj,
                   '' AS resultado_setfocus, @operacion_id AS OperacionId;
        END CATCH
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_USU_Clave_Forzada_Cambiar
(
    @usu_id varchar(10), @clave_nueva varchar(128),
    @adm_id varchar(4) = NULL, @ip varchar(45) = NULL,
    @origen varchar(20) = 'GC.SITIO', @operacion_id uniqueidentifier = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @resultado smallint = -1, @resultado_id varchar(40) = 'CLAVE_FORZADA_ERROR',
            @resultado_msj varchar(250) = 'No se pudo establecer la nueva contraseña.',
            @resultado_setfocus varchar(40) = '', @ahora datetime2(0) = SYSDATETIME(),
            @guardada varchar(300), @cifrada_nueva varchar(300), @json nvarchar(max), @longitud int,
            @obligatoria bit, @vencimiento datetime2(0), @operacion_activa uniqueidentifier,
            @val_long bit, @min smallint, @max smallint, @val_complejidad bit,
            @mayus bit, @minus bit, @numero bit, @simbolo bit, @distinta bit,
            @dias smallint, @auditar bit;

    SET @usu_id = LTRIM(RTRIM(ISNULL(@usu_id, '')));
    SET @origen = LEFT(ISNULL(NULLIF(LTRIM(RTRIM(@origen)), ''), 'GC.SITIO'), 20);

    SELECT @val_long = seg_validar_longitud, @min = seg_longitud_minima,
           @max = seg_longitud_maxima, @val_complejidad = seg_validar_complejidad,
           @mayus = seg_requiere_mayuscula, @minus = seg_requiere_minuscula,
           @numero = seg_requiere_numero, @simbolo = seg_requiere_simbolo,
           @distinta = seg_impedir_clave_actual, @dias = seg_dias_vigencia,
           @auditar = seg_auditoria_activa
    FROM dbo.seguridad_configuracion WHERE seg_id = 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @guardada = usu_password, @obligatoria = usu_cambio_clave_obligatorio,
               @vencimiento = usu_cambio_clave_vencimiento,
               @operacion_activa = usu_cambio_clave_operacion_id
          FROM dbo.usuarios WITH (UPDLOCK, HOLDLOCK) WHERE usu_id = @usu_id;
        -- La operación de cambio definitivo continúa la misma trazabilidad del blanqueo.
        -- El identificador recibido sólo se utiliza como respaldo para estados legados.
        SET @operacion_id = ISNULL(@operacion_activa, @operacion_id);
        SET @operacion_id = ISNULL(@operacion_id, NEWID());

        IF @guardada IS NULL
        BEGIN SET @resultado = 1; SET @resultado_id = 'USUARIO_INEXISTENTE'; SET @resultado_msj = 'El usuario no existe.'; END
        ELSE IF ISNULL(@obligatoria, 0) = 0
        BEGIN SET @resultado = 2; SET @resultado_id = 'CAMBIO_NO_REQUERIDO'; SET @resultado_msj = 'El usuario no posee un cambio obligatorio pendiente.'; END
        ELSE IF @vencimiento IS NOT NULL AND @ahora >= @vencimiento
        BEGIN SET @resultado = 3; SET @resultado_id = 'CLAVE_TEMPORAL_VENCIDA'; SET @resultado_msj = 'La contraseña temporal ha vencido. Solicite un nuevo blanqueo.'; END
        ELSE IF @clave_nueva IS NULL OR DATALENGTH(@clave_nueva) = 0
             OR LEN(REPLACE(REPLACE(REPLACE(REPLACE(@clave_nueva, ' ', ''), CHAR(9), ''), CHAR(13), ''), CHAR(10), '')) = 0
        BEGIN SET @resultado = 4; SET @resultado_id = 'CLAVE_NUEVA_REQUERIDA'; SET @resultado_msj = 'Debe ingresar una contraseña nueva.'; END

        SET @longitud = DATALENGTH(ISNULL(@clave_nueva, ''));
        IF @resultado = -1 AND @val_long = 1 AND (@longitud < @min OR @longitud > @max)
        BEGIN SET @resultado = 5; SET @resultado_id = 'LONGITUD_INVALIDA'; SET @resultado_msj = CONCAT('La contraseña debe tener entre ', @min, ' y ', @max, ' caracteres.'); END
        IF @resultado = -1 AND @val_complejidad = 1 AND @mayus = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[A-Z]%'
        BEGIN SET @resultado = 6; SET @resultado_id = 'FALTA_MAYUSCULA'; SET @resultado_msj = 'La contraseña debe incluir al menos una letra mayúscula.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @minus = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[a-z]%'
        BEGIN SET @resultado = 7; SET @resultado_id = 'FALTA_MINUSCULA'; SET @resultado_msj = 'La contraseña debe incluir al menos una letra minúscula.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @numero = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[0-9]%'
        BEGIN SET @resultado = 8; SET @resultado_id = 'FALTA_NUMERO'; SET @resultado_msj = 'La contraseña debe incluir al menos un número.'; END
        IF @resultado = -1 AND @val_complejidad = 1 AND @simbolo = 1 AND @clave_nueva COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Za-z0-9 ]%'
        BEGIN SET @resultado = 9; SET @resultado_id = 'FALTA_SIMBOLO'; SET @resultado_msj = 'La contraseña debe incluir al menos un símbolo.'; END

        IF @resultado = -1
        BEGIN
            SELECT @json = (SELECT @usu_id AS usuario, @clave_nueva AS clave FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            SET @cifrada_nueva = dbo.SF_Pass_E(@json);
            IF @distinta = 1 AND @guardada = @cifrada_nueva
            BEGIN SET @resultado = 10; SET @resultado_id = 'CLAVE_REPETIDA'; SET @resultado_msj = 'La contraseña definitiva debe ser diferente de la contraseña temporal.'; END
        END

        IF @resultado <> -1
        BEGIN
            SET @resultado_setfocus = 'ClaveNueva';
            ROLLBACK TRANSACTION;
            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_usu_ejecutor, usa_fecha, usa_evento, usa_resultado, usa_ip,
                 adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id, @usu_id, @ahora, @resultado_id, @resultado, @ip,
                        @adm_id, @origen, @resultado_msj, @operacion_id);
        END
        ELSE
        BEGIN
            UPDATE dbo.usuarios
               SET usu_password = @cifrada_nueva,
                   usu_cambio_clave_obligatorio = 0,
                   usu_cambio_clave_motivo = NULL,
                   usu_cambio_clave_fecha = NULL,
                   usu_cambio_clave_vencimiento = NULL,
                   usu_cambio_clave_operacion_id = NULL,
                   usu_version_credencial = usu_version_credencial + 1,
                   usu_expira = 1,
                   usu_dias_expiracion = @dias,
                   usu_fecha_expira_inicio = @ahora
             WHERE usu_id = @usu_id;
            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_usu_ejecutor, usa_fecha, usa_evento, usa_resultado, usa_ip,
                 adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id, @usu_id, @ahora, 'CLAVE_FORZADA_CAMBIO_OK', 0, @ip,
                        @adm_id, @origen, 'El usuario estableció su contraseña definitiva.', @operacion_id);
            COMMIT TRANSACTION;
            SET @resultado = 0; SET @resultado_id = 'CLAVE_FORZADA_CAMBIO_OK';
            SET @resultado_msj = 'La contraseña se modificó correctamente.';
        END
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SET @resultado = -1; SET @resultado_id = 'CLAVE_FORZADA_ERROR';
        SET @resultado_msj = 'No se pudo establecer la nueva contraseña.';
    END CATCH

    SELECT @resultado AS resultado, @resultado_id AS resultado_id,
           @resultado_msj AS resultado_msj, @resultado_setfocus AS resultado_setfocus,
           @operacion_id AS OperacionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_USU_Desbloquear
(
    @usu_id_objetivo varchar(10), @usu_id_ejecutor varchar(10),
    @adm_id varchar(4) = NULL, @ip varchar(45) = NULL,
    @origen varchar(20) = 'GC.SITIO', @operacion_id uniqueidentifier = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @ahora datetime2(0) = SYSDATETIME(), @auditar bit;
    SET @usu_id_objetivo = LTRIM(RTRIM(ISNULL(@usu_id_objetivo, '')));
    SET @usu_id_ejecutor = LTRIM(RTRIM(ISNULL(@usu_id_ejecutor, '')));
    SET @operacion_id = ISNULL(@operacion_id, NEWID());
    SELECT @auditar = seg_auditoria_activa FROM dbo.seguridad_configuracion WHERE seg_id = 1;

    IF @usu_id_objetivo = @usu_id_ejecutor
        SELECT 1 AS resultado, 'AUTODESBLOQUEO_NO_PERMITIDO' AS resultado_id,
               'No puede desbloquear su propio usuario.' AS resultado_msj, '' AS resultado_setfocus,
               @operacion_id AS OperacionId;
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE usu_id = @usu_id_objetivo)
        SELECT 2 AS resultado, 'USUARIO_INEXISTENTE' AS resultado_id,
               'El usuario seleccionado no existe.' AS resultado_msj, '' AS resultado_setfocus,
               @operacion_id AS OperacionId;
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.usuarios WHERE usu_id = @usu_id_objetivo AND usu_bloqueado = 1)
        SELECT 3 AS resultado, 'USUARIO_NO_BLOQUEADO' AS resultado_id,
               'El usuario seleccionado no está bloqueado.' AS resultado_msj, '' AS resultado_setfocus,
               @operacion_id AS OperacionId;
    ELSE
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;
            UPDATE dbo.usuarios WITH (UPDLOCK)
               SET usu_bloqueado = 0, usu_bloqueado_fecha = NULL, usu_intentos = 0
             WHERE usu_id = @usu_id_objetivo AND usu_bloqueado = 1;
            IF @auditar = 1 INSERT dbo.usuarios_auditoria
                (usu_id, usa_usu_ejecutor, usa_fecha, usa_evento, usa_resultado, usa_ip,
                 adm_id, usa_origen, usa_detalle, usa_operacion_id)
                VALUES (@usu_id_objetivo, @usu_id_ejecutor, @ahora, 'USUARIO_DESBLOQUEADO', 0, @ip,
                        @adm_id, @origen, 'Se limpiaron el bloqueo y los intentos fallidos.', @operacion_id);
            COMMIT TRANSACTION;
            SELECT 0 AS resultado, 'USUARIO_DESBLOQUEADO' AS resultado_id,
                   'El usuario fue desbloqueado correctamente.' AS resultado_msj,
                   '' AS resultado_setfocus, @operacion_id AS OperacionId;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SELECT -1 AS resultado, 'DESBLOQUEO_ERROR' AS resultado_id,
                   'No se pudo desbloquear el usuario.' AS resultado_msj,
                   '' AS resultado_setfocus, @operacion_id AS OperacionId;
        END CATCH
    END
END
GO
