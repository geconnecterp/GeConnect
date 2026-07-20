USE [geco_0000];
GO

-- Aplicación incremental para instalaciones existentes.
-- Impide que el solicitante vea, tome o resuelva su propia autorización.

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_PENDIENTES
    @IdUsuario VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id,
        s.IdSolicitudExterna,
        s.DerCodigo,
        d.der_descripcion AS DerechoDescripcion,
        s.Estado,
        s.IdUsuarioSolicitante,
        s.CodigoModuloOrigen,
        s.FechaSolicitud,
        s.TimeoutSegundos,
        s.FechaExpiracion,
        s.DecisionPorDefecto,
        s.CodigoResolucionPorDefecto,
        s.MensajeResolucionPorDefecto,
        s.ContextoJson,
        s.IdUsuarioBloqueo,
        s.FechaBloqueo,
        CAST(1 AS BIT) AS PuedeAutorizar
    FROM dbo.sauth_SolicitudesAutorizacion s
    INNER JOIN dbo.uderechos d
        ON d.der_codigo = s.DerCodigo
    INNER JOIN dbo.usuarios_uderechos ud
        ON ud.der_codigo = s.DerCodigo
       AND ud.usu_id = @IdUsuario
    WHERE UPPER(LTRIM(RTRIM(s.IdUsuarioSolicitante))) <>
          UPPER(LTRIM(RTRIM(@IdUsuario)))
      AND
      (
          s.Estado = 'PENDIENTE'
          OR (s.Estado = 'EN_PROCESO' AND s.IdUsuarioBloqueo = @IdUsuario)
          OR (s.Estado = 'EN_PROCESO' AND s.FechaBloqueo < DATEADD(SECOND, -60, SYSUTCDATETIME()))
      );
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_BLOQUEAR
    @IdSolicitud UNIQUEIDENTIFIER,
    @IdUsuario VARCHAR(100),
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.sauth_SolicitudesAutorizacion
       SET Estado = 'EN_PROCESO',
           IdUsuarioBloqueo = @IdUsuario,
           FechaBloqueo = SYSUTCDATETIME()
     WHERE Id = @IdSolicitud
       AND UPPER(LTRIM(RTRIM(IdUsuarioSolicitante))) <>
           UPPER(LTRIM(RTRIM(@IdUsuario)))
       AND
       (
           Estado = 'PENDIENTE'
           OR (Estado = 'EN_PROCESO' AND FechaBloqueo < DATEADD(SECOND, -60, SYSUTCDATETIME()))
       );

    SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_RESOLVER
    @IdResolucion UNIQUEIDENTIFIER,
    @IdSolicitud UNIQUEIDENTIFIER,
    @Decision VARCHAR(50),
    @CodigoResolucion VARCHAR(50),
    @Mensaje NVARCHAR(500),
    @IdUsuarioResolucion VARCHAR(100),
    @EsResolucionPorDefecto BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_SolicitudesAutorizacion
        WHERE Id = @IdSolicitud
          AND UPPER(LTRIM(RTRIM(IdUsuarioSolicitante))) =
              UPPER(LTRIM(RTRIM(@IdUsuarioResolucion)))
    )
        THROW 50012, 'El usuario solicitante no puede autorizar su propia solicitud.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_SolicitudesAutorizacion
        WHERE Id = @IdSolicitud
          AND Estado IN ('EXPIRADO', 'RESUELTO')
    )
        THROW 50010, 'La solicitud ya fue resuelta o expiró.', 1;

    INSERT dbo.sauth_ResolucionesAutorizacion
    (
        Id, IdSolicitud, Decision, CodigoResolucion, Mensaje,
        IdUsuarioResolucion, FechaResolucion, EsResolucionPorDefecto
    )
    VALUES
    (
        @IdResolucion, @IdSolicitud, @Decision, @CodigoResolucion, @Mensaje,
        @IdUsuarioResolucion, SYSUTCDATETIME(), @EsResolucionPorDefecto
    );

    UPDATE dbo.sauth_SolicitudesAutorizacion
       SET Estado = 'RESUELTO'
     WHERE Id = @IdSolicitud;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_OBTENER
    @IdSolicitud UNIQUEIDENTIFIER,
    @IdUsuario VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.*,
        d.der_descripcion AS DerechoDescripcion,
        CASE
            WHEN ud.der_codigo IS NULL
                 OR UPPER(LTRIM(RTRIM(s.IdUsuarioSolicitante))) =
                    UPPER(LTRIM(RTRIM(@IdUsuario)))
                THEN CAST(0 AS BIT)
            ELSE CAST(1 AS BIT)
        END AS PuedeAutorizar
    FROM dbo.sauth_SolicitudesAutorizacion s
    INNER JOIN dbo.uderechos d
        ON d.der_codigo = s.DerCodigo
    LEFT JOIN dbo.usuarios_uderechos ud
        ON ud.der_codigo = s.DerCodigo
       AND ud.usu_id = @IdUsuario
    WHERE s.Id = @IdSolicitud;

    SELECT *
    FROM dbo.sauth_ResolucionesAutorizacion
    WHERE IdSolicitud = @IdSolicitud;
END;
GO
