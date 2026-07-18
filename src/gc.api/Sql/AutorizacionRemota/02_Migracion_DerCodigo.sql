USE [geco_0000];
GO

SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.uderechos', N'U') IS NULL
    THROW 50001, 'No existe la tabla corporativa dbo.uderechos.', 1;

IF OBJECT_ID(N'dbo.usuarios_uderechos', N'U') IS NULL
    THROW 50002, 'No existe la tabla corporativa dbo.usuarios_uderechos.', 1;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion', N'U') IS NULL
        THROW 50003, 'No existe dbo.sauth_SolicitudesAutorizacion. Use la instalación limpia.', 1;

    IF COL_LENGTH(N'dbo.sauth_SolicitudesAutorizacion', N'DerCodigo') IS NULL
    BEGIN
        ALTER TABLE dbo.sauth_SolicitudesAutorizacion ADD DerCodigo SMALLINT NULL;
    END;

    IF COL_LENGTH(N'dbo.sauth_SolicitudesAutorizacion', N'CodigoTipo') IS NOT NULL
    BEGIN
        IF OBJECT_ID(N'dbo.sauth_TiposAutorizacion', N'U') IS NULL
            THROW 50004, 'No existe el catálogo anterior para convertir CodigoTipo a DerCodigo.', 1;

        UPDATE s
           SET DerCodigo = t.der_codigo
        FROM dbo.sauth_SolicitudesAutorizacion s
        INNER JOIN dbo.sauth_TiposAutorizacion t
            ON t.CodigoTipo = s.CodigoTipo
        WHERE s.DerCodigo IS NULL;

        IF EXISTS (SELECT 1 FROM dbo.sauth_SolicitudesAutorizacion WHERE DerCodigo IS NULL)
            THROW 50005, 'Hay solicitudes cuyo CodigoTipo no tiene der_codigo asociado.', 1;

        DECLARE @dropForeignKeys NVARCHAR(MAX) = N'';
        SELECT @dropForeignKeys +=
            N'ALTER TABLE dbo.sauth_SolicitudesAutorizacion DROP CONSTRAINT '
            + QUOTENAME(fk.name) + N';'
        FROM sys.foreign_keys fk
        WHERE fk.parent_object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion')
          AND fk.referenced_object_id = OBJECT_ID(N'dbo.sauth_TiposAutorizacion');

        IF @dropForeignKeys <> N'' EXEC sys.sp_executesql @dropForeignKeys;

        ALTER TABLE dbo.sauth_SolicitudesAutorizacion DROP COLUMN CodigoTipo;
    END;

    IF EXISTS (SELECT 1 FROM dbo.sauth_SolicitudesAutorizacion WHERE DerCodigo IS NULL)
        THROW 50006, 'Hay solicitudes sin DerCodigo.', 1;

    ALTER TABLE dbo.sauth_SolicitudesAutorizacion ALTER COLUMN DerCodigo SMALLINT NOT NULL;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_SolicitudesAutorizacion s
        LEFT JOIN dbo.uderechos d ON d.der_codigo = s.DerCodigo
        WHERE d.der_codigo IS NULL
    )
        THROW 50007, 'Hay solicitudes con DerCodigo inexistente en uderechos.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_sauth_Solicitud_uDerechos'
          AND parent_object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion')
    )
    BEGIN
        ALTER TABLE dbo.sauth_SolicitudesAutorizacion WITH CHECK
        ADD CONSTRAINT FK_sauth_Solicitud_uDerechos
            FOREIGN KEY (DerCodigo) REFERENCES dbo.uderechos(der_codigo);
    END;

    IF OBJECT_ID(N'dbo.sauth_BandejaSalida', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.sauth_BandejaSalida
        (
            Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_sauth_BandejaSalida PRIMARY KEY,
            Tipo NVARCHAR(100) NOT NULL,
            PayloadJson NVARCHAR(MAX) NOT NULL,
            FechaOcurrencia DATETIME2(7) NOT NULL,
            FechaProcesado DATETIME2(7) NULL,
            Intentos INT NOT NULL CONSTRAINT DF_sauth_BandejaSalida_Intentos DEFAULT 0,
            Error NVARCHAR(MAX) NULL,
            CONSTRAINT CK_sauth_BandejaSalida_Intentos CHECK (Intentos >= 0)
        );
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_SolicitudesAutorizacion
        GROUP BY CodigoModuloOrigen, IdempotencyKey
        HAVING COUNT(*) > 1
    )
        THROW 50008, 'Existen claves de idempotencia duplicadas; deben resolverse antes de crear el índice único.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.sauth_ResolucionesAutorizacion
        GROUP BY IdSolicitud
        HAVING COUNT(*) > 1
    )
        THROW 50009, 'Existen solicitudes con más de una resolución.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_sauth_Solicitudes_Idempotencia' AND object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion'))
        CREATE UNIQUE INDEX UX_sauth_Solicitudes_Idempotencia
            ON dbo.sauth_SolicitudesAutorizacion(CodigoModuloOrigen, IdempotencyKey);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_sauth_Resoluciones_IdSolicitud' AND object_id = OBJECT_ID(N'dbo.sauth_ResolucionesAutorizacion'))
        CREATE UNIQUE INDEX UX_sauth_Resoluciones_IdSolicitud
            ON dbo.sauth_ResolucionesAutorizacion(IdSolicitud);

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_sauth_Solicitudes_Pendientes' AND object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion'))
        DROP INDEX IX_sauth_Solicitudes_Pendientes ON dbo.sauth_SolicitudesAutorizacion;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_sauth_Solicitudes_Historico' AND object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion'))
        DROP INDEX IX_sauth_Solicitudes_Historico ON dbo.sauth_SolicitudesAutorizacion;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_sauth_Solicitudes_EstadoFecha' AND object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion'))
        CREATE INDEX IX_sauth_Solicitudes_EstadoFecha
            ON dbo.sauth_SolicitudesAutorizacion(Estado, FechaSolicitud)
            INCLUDE
            (
                DerCodigo, IdUsuarioBloqueo, FechaBloqueo, FechaExpiracion,
                IdUsuarioSolicitante, CodigoModuloOrigen
            );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_sauth_Solicitudes_Expiracion' AND object_id = OBJECT_ID(N'dbo.sauth_SolicitudesAutorizacion'))
        CREATE INDEX IX_sauth_Solicitudes_Expiracion
            ON dbo.sauth_SolicitudesAutorizacion(Estado, FechaExpiracion)
            INCLUDE (IdUsuarioBloqueo);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_sauth_BandejaSalida_Pendientes' AND object_id = OBJECT_ID(N'dbo.sauth_BandejaSalida'))
        CREATE INDEX IX_sauth_BandejaSalida_Pendientes
            ON dbo.sauth_BandejaSalida(FechaOcurrencia, Intentos)
            INCLUDE (Tipo)
            WHERE FechaProcesado IS NULL;

    DROP TABLE IF EXISTS dbo.sauth_TiposAutorizacion;
    DROP TABLE IF EXISTS dbo.sauth_CategoriasAutorizacion;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
