USE [geco_0000];
GO

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
    WHERE s.Estado = 'PENDIENTE'
       OR (s.Estado = 'EN_PROCESO' AND s.IdUsuarioBloqueo = @IdUsuario)
       OR (s.Estado = 'EN_PROCESO' AND s.FechaBloqueo < DATEADD(SECOND, -60, SYSUTCDATETIME()));
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
       AND
       (
           Estado = 'PENDIENTE'
           OR (Estado = 'EN_PROCESO' AND FechaBloqueo < DATEADD(SECOND, -60, SYSUTCDATETIME()))
       );

    SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_CREAR
    @Id UNIQUEIDENTIFIER,
    @IdSolicitudExterna VARCHAR(100),
    @DerCodigo SMALLINT,
    @Estado VARCHAR(50),
    @IdUsuarioSolicitante VARCHAR(100),
    @CodigoModuloOrigen VARCHAR(50),
    @FechaSolicitud DATETIME2(7),
    @TimeoutSegundos INT,
    @FechaExpiracion DATETIME2(7),
    @DecisionPorDefecto VARCHAR(50),
    @CodigoResolucionPorDefecto VARCHAR(50),
    @MensajeResolucionPorDefecto NVARCHAR(500),
    @ContextoJson NVARCHAR(MAX),
    @IdempotencyKey VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_SolicitudesAutorizacion
        WHERE IdempotencyKey = @IdempotencyKey
          AND CodigoModuloOrigen = @CodigoModuloOrigen
    )
    BEGIN
        SELECT TOP (1) *
        FROM dbo.sauth_SolicitudesAutorizacion
        WHERE IdempotencyKey = @IdempotencyKey
          AND CodigoModuloOrigen = @CodigoModuloOrigen;
        RETURN;
    END;

    INSERT dbo.sauth_SolicitudesAutorizacion
    (
        Id, IdSolicitudExterna, DerCodigo, Estado, IdUsuarioSolicitante,
        CodigoModuloOrigen, FechaSolicitud, TimeoutSegundos, FechaExpiracion,
        DecisionPorDefecto, CodigoResolucionPorDefecto, MensajeResolucionPorDefecto,
        ContextoJson, IdempotencyKey
    )
    VALUES
    (
        @Id, @IdSolicitudExterna, @DerCodigo, @Estado, @IdUsuarioSolicitante,
        @CodigoModuloOrigen, @FechaSolicitud, @TimeoutSegundos, @FechaExpiracion,
        @DecisionPorDefecto, @CodigoResolucionPorDefecto, @MensajeResolucionPorDefecto,
        @ContextoJson, @IdempotencyKey
    );

    SELECT * FROM dbo.sauth_SolicitudesAutorizacion WHERE Id = @Id;
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
        SELECT 1 FROM dbo.sauth_SolicitudesAutorizacion
        WHERE Id = @IdSolicitud AND Estado IN ('EXPIRADO', 'RESUELTO')
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
        CASE WHEN ud.der_codigo IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS PuedeAutorizar
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

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_OBTENER_EXPIRADAS
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.sauth_SolicitudesAutorizacion
    WHERE Estado = 'PENDIENTE'
      AND FechaExpiracion <= SYSUTCDATETIME()
      AND IdUsuarioBloqueo IS NULL;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_EXPIRAR
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
        SELECT 1 FROM dbo.sauth_SolicitudesAutorizacion
        WHERE Id = @IdSolicitud AND Estado IN ('EXPIRADO', 'RESUELTO')
    )
        THROW 50011, 'La solicitud ya fue resuelta o expiró.', 1;

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
       SET Estado = 'EXPIRADO'
     WHERE Id = @IdSolicitud;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_SOLICITUD_AUTORIZACION_HISTORICO
    @FechaDesde DATETIME2(7),
    @FechaHasta DATETIME2(7),
    @Top INT,
    @IdUsuario VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Top)
        s.*,
        d.der_descripcion AS DerechoDescripcion,
        CAST(1 AS BIT) AS PuedeAutorizar
    FROM dbo.sauth_SolicitudesAutorizacion s
    INNER JOIN dbo.uderechos d
        ON d.der_codigo = s.DerCodigo
    INNER JOIN dbo.usuarios_uderechos ud
        ON ud.der_codigo = s.DerCodigo
       AND ud.usu_id = @IdUsuario
    WHERE s.Estado IN ('RESUELTO', 'EXPIRADO')
      AND s.FechaSolicitud >= @FechaDesde
      AND s.FechaSolicitud <= @FechaHasta
    ORDER BY s.FechaSolicitud DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_RESOLUCION_AUTORIZACION_HISTORICO
    @FechaDesde DATETIME2(7),
    @FechaHasta DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*
    FROM dbo.sauth_ResolucionesAutorizacion r
    INNER JOIN dbo.sauth_SolicitudesAutorizacion s
        ON s.Id = r.IdSolicitud
    WHERE s.FechaSolicitud >= @FechaDesde
      AND s.FechaSolicitud <= @FechaHasta;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_BANDEJA_SALIDA_INSERTAR
    @Id UNIQUEIDENTIFIER,
    @Tipo NVARCHAR(100),
    @PayloadJson NVARCHAR(MAX),
    @FechaOcurrencia DATETIME2(7),
    @Intentos INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT dbo.sauth_BandejaSalida
        (Id, Tipo, PayloadJson, FechaOcurrencia, Intentos)
    VALUES
        (@Id, @Tipo, @PayloadJson, @FechaOcurrencia, @Intentos);
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_BANDEJA_SALIDA_OBTENER_PENDIENTES
    @BatchSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@BatchSize)
        Id, Tipo, PayloadJson, FechaOcurrencia, FechaProcesado, Intentos, Error
    FROM dbo.sauth_BandejaSalida
    WHERE FechaProcesado IS NULL
      AND Intentos < 5
    ORDER BY FechaOcurrencia;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_SAUTH_BANDEJA_SALIDA_ACTUALIZAR
    @Id UNIQUEIDENTIFIER,
    @FechaProcesado DATETIME2(7) = NULL,
    @Intentos INT,
    @Error NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.sauth_BandejaSalida
       SET FechaProcesado = @FechaProcesado,
           Intentos = @Intentos,
           Error = @Error
     WHERE Id = @Id;
END;
GO
