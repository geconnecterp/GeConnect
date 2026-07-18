USE [geco_0000];
GO

SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.uderechos', N'U') IS NULL
    THROW 50001, 'No existe la tabla corporativa dbo.uderechos.', 1;

IF OBJECT_ID(N'dbo.usuarios_uderechos', N'U') IS NULL
    THROW 50002, 'No existe la tabla corporativa dbo.usuarios_uderechos.', 1;
GO

-- ADVERTENCIA: instalación limpia; elimina los datos existentes del módulo.
DROP TABLE IF EXISTS dbo.sauth_ResolucionesAutorizacion;
DROP TABLE IF EXISTS dbo.sauth_SolicitudesAutorizacion;
DROP TABLE IF EXISTS dbo.sauth_BandejaSalida;
DROP TABLE IF EXISTS dbo.sauth_TiposAutorizacion;
DROP TABLE IF EXISTS dbo.sauth_CategoriasAutorizacion;
GO

CREATE TABLE dbo.sauth_SolicitudesAutorizacion
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_sauth_SolicitudesAutorizacion PRIMARY KEY
        CONSTRAINT DF_sauth_SolicitudesAutorizacion_Id DEFAULT NEWID(),
    IdSolicitudExterna VARCHAR(100) NOT NULL,
    DerCodigo SMALLINT NOT NULL,
    Estado VARCHAR(50) NOT NULL,
    IdUsuarioSolicitante VARCHAR(100) NOT NULL,
    CodigoModuloOrigen VARCHAR(50) NOT NULL,
    FechaSolicitud DATETIME2(7) NOT NULL
        CONSTRAINT DF_sauth_SolicitudesAutorizacion_FechaSolicitud DEFAULT SYSUTCDATETIME(),
    TimeoutSegundos INT NOT NULL,
    FechaExpiracion DATETIME2(7) NOT NULL,
    DecisionPorDefecto VARCHAR(50) NOT NULL,
    CodigoResolucionPorDefecto VARCHAR(50) NOT NULL,
    MensajeResolucionPorDefecto NVARCHAR(500) NULL,
    ContextoJson NVARCHAR(MAX) NULL,
    IdempotencyKey VARCHAR(100) NOT NULL,
    IdUsuarioBloqueo VARCHAR(100) NULL,
    FechaBloqueo DATETIME2(7) NULL,

    CONSTRAINT FK_sauth_Solicitud_uDerechos
        FOREIGN KEY (DerCodigo) REFERENCES dbo.uderechos(der_codigo),
    CONSTRAINT CK_sauth_Solicitud_Estado
        CHECK (Estado IN ('PENDIENTE', 'EN_PROCESO', 'EXPIRADO', 'RESUELTO')),
    CONSTRAINT CK_sauth_Solicitud_DecisionPorDefecto
        CHECK (DecisionPorDefecto IN ('APROBADO', 'RECHAZADO')),
    CONSTRAINT CK_sauth_Solicitud_Timeout
        CHECK (TimeoutSegundos > 0)
);
GO

CREATE TABLE dbo.sauth_ResolucionesAutorizacion
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_sauth_ResolucionesAutorizacion PRIMARY KEY
        CONSTRAINT DF_sauth_ResolucionesAutorizacion_Id DEFAULT NEWID(),
    IdSolicitud UNIQUEIDENTIFIER NOT NULL,
    Decision VARCHAR(50) NOT NULL,
    CodigoResolucion VARCHAR(50) NOT NULL,
    Mensaje NVARCHAR(500) NULL,
    IdUsuarioResolucion VARCHAR(100) NOT NULL,
    FechaResolucion DATETIME2(7) NOT NULL
        CONSTRAINT DF_sauth_ResolucionesAutorizacion_FechaResolucion DEFAULT SYSUTCDATETIME(),
    EsResolucionPorDefecto BIT NOT NULL
        CONSTRAINT DF_sauth_ResolucionesAutorizacion_EsDefecto DEFAULT 0,

    CONSTRAINT FK_sauth_Resolucion_Solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES dbo.sauth_SolicitudesAutorizacion(Id),
    CONSTRAINT CK_sauth_Resolucion_Decision
        CHECK (Decision IN ('APROBADO', 'RECHAZADO'))
);
GO

CREATE TABLE dbo.sauth_BandejaSalida
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_sauth_BandejaSalida PRIMARY KEY,
    Tipo NVARCHAR(100) NOT NULL,
    PayloadJson NVARCHAR(MAX) NOT NULL,
    FechaOcurrencia DATETIME2(7) NOT NULL,
    FechaProcesado DATETIME2(7) NULL,
    Intentos INT NOT NULL
        CONSTRAINT DF_sauth_BandejaSalida_Intentos DEFAULT 0,
    Error NVARCHAR(MAX) NULL,

    CONSTRAINT CK_sauth_BandejaSalida_Intentos CHECK (Intentos >= 0)
);
GO

CREATE UNIQUE INDEX UX_sauth_Solicitudes_Idempotencia
    ON dbo.sauth_SolicitudesAutorizacion(CodigoModuloOrigen, IdempotencyKey);

CREATE UNIQUE INDEX UX_sauth_Resoluciones_IdSolicitud
    ON dbo.sauth_ResolucionesAutorizacion(IdSolicitud);

CREATE INDEX IX_sauth_Solicitudes_EstadoFecha
    ON dbo.sauth_SolicitudesAutorizacion(Estado, FechaSolicitud)
    INCLUDE
    (
        DerCodigo, IdUsuarioBloqueo, FechaBloqueo, FechaExpiracion,
        IdUsuarioSolicitante, CodigoModuloOrigen
    );

CREATE INDEX IX_sauth_Solicitudes_Expiracion
    ON dbo.sauth_SolicitudesAutorizacion(Estado, FechaExpiracion)
    INCLUDE (IdUsuarioBloqueo);

CREATE INDEX IX_sauth_BandejaSalida_Pendientes
    ON dbo.sauth_BandejaSalida(FechaOcurrencia, Intentos)
    INCLUDE (Tipo)
    WHERE FechaProcesado IS NULL;
GO
