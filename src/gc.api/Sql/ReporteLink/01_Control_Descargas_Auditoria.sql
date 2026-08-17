USE [geco_0000]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    Control de descargas y auditoría de enlaces públicos de reportes.

    Política inicial propuesta:
      - El enlace puede iniciarse hasta FechaExpiracionUtc.
      - El primer intento válido abre una ventana configurable (60 minutos).
      - Se permiten hasta MaxDescargas descargas confirmadas (5 por defecto).
      - Los intentos y las descargas completadas se registran por separado.

    El script es idempotente y conserva Usado/FechaUsoUtc para compatibilidad.
*/

IF COL_LENGTH(N'dbo.ReporteLink', N'MaxDescargas') IS NULL
    ALTER TABLE dbo.ReporteLink ADD MaxDescargas SMALLINT NULL;
GO
IF COL_LENGTH(N'dbo.ReporteLink', N'CantidadDescargas') IS NULL
    ALTER TABLE dbo.ReporteLink ADD CantidadDescargas SMALLINT NULL;
GO
IF COL_LENGTH(N'dbo.ReporteLink', N'VentanaDescargaMinutos') IS NULL
    ALTER TABLE dbo.ReporteLink ADD VentanaDescargaMinutos SMALLINT NULL;
GO
IF COL_LENGTH(N'dbo.ReporteLink', N'FechaPrimerIntentoUtc') IS NULL
    ALTER TABLE dbo.ReporteLink ADD FechaPrimerIntentoUtc DATETIME2(3) NULL;
GO
IF COL_LENGTH(N'dbo.ReporteLink', N'FechaVentanaHastaUtc') IS NULL
    ALTER TABLE dbo.ReporteLink ADD FechaVentanaHastaUtc DATETIME2(3) NULL;
GO
IF COL_LENGTH(N'dbo.ReporteLink', N'FechaUltimaDescargaUtc') IS NULL
    ALTER TABLE dbo.ReporteLink ADD FechaUltimaDescargaUtc DATETIME2(3) NULL;
GO

UPDATE dbo.ReporteLink
   SET MaxDescargas = ISNULL(MaxDescargas, 5),
       CantidadDescargas = ISNULL(CantidadDescargas, CASE WHEN Usado = 1 THEN 1 ELSE 0 END),
       VentanaDescargaMinutos = ISNULL(VentanaDescargaMinutos, 60),
       FechaPrimerIntentoUtc = COALESCE(FechaPrimerIntentoUtc, FechaUsoUtc),
       FechaUltimaDescargaUtc = COALESCE(FechaUltimaDescargaUtc, FechaUsoUtc),
       FechaVentanaHastaUtc = COALESCE(FechaVentanaHastaUtc,
           CASE WHEN FechaUsoUtc IS NOT NULL THEN DATEADD(MINUTE, 60, FechaUsoUtc) ELSE NULL END);
GO

ALTER TABLE dbo.ReporteLink ALTER COLUMN MaxDescargas SMALLINT NOT NULL;
ALTER TABLE dbo.ReporteLink ALTER COLUMN CantidadDescargas SMALLINT NOT NULL;
ALTER TABLE dbo.ReporteLink ALTER COLUMN VentanaDescargaMinutos SMALLINT NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.ReporteLink') AND name = N'DF_ReporteLink_MaxDescargas')
    ALTER TABLE dbo.ReporteLink ADD CONSTRAINT DF_ReporteLink_MaxDescargas DEFAULT (5) FOR MaxDescargas;
GO
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.ReporteLink') AND name = N'DF_ReporteLink_CantidadDescargas')
    ALTER TABLE dbo.ReporteLink ADD CONSTRAINT DF_ReporteLink_CantidadDescargas DEFAULT (0) FOR CantidadDescargas;
GO
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.ReporteLink') AND name = N'DF_ReporteLink_VentanaDescargaMinutos')
    ALTER TABLE dbo.ReporteLink ADD CONSTRAINT DF_ReporteLink_VentanaDescargaMinutos DEFAULT (60) FOR VentanaDescargaMinutos;
GO

IF OBJECT_ID(N'dbo.ReporteLinkAuditoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReporteLinkAuditoria
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReporteLinkAuditoria PRIMARY KEY,
        ReporteLinkId BIGINT NULL,
        Codigo VARCHAR(20) NOT NULL,
        FechaUtc DATETIME2(3) NOT NULL CONSTRAINT DF_ReporteLinkAuditoria_FechaUtc DEFAULT SYSUTCDATETIME(),
        Evento VARCHAR(40) NOT NULL,
        ResultadoHttp SMALLINT NULL,
        Ip VARCHAR(45) NULL,
        UserAgent NVARCHAR(500) NULL,
        Referer NVARCHAR(1000) NULL,
        Bytes BIGINT NULL,
        DuracionMs INT NULL,
        Detalle NVARCHAR(500) NULL,
        CONSTRAINT FK_ReporteLinkAuditoria_ReporteLink
            FOREIGN KEY (ReporteLinkId) REFERENCES dbo.ReporteLink(Id) ON DELETE SET NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ReporteLinkAuditoria') AND name = N'IX_ReporteLinkAuditoria_CodigoFecha')
    CREATE INDEX IX_ReporteLinkAuditoria_CodigoFecha ON dbo.ReporteLinkAuditoria(Codigo, FechaUtc DESC)
        INCLUDE(Evento, ResultadoHttp, DuracionMs, Bytes);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ReporteLinkAuditoria') AND name = N'IX_ReporteLinkAuditoria_EventoFecha')
    CREATE INDEX IX_ReporteLinkAuditoria_EventoFecha ON dbo.ReporteLinkAuditoria(Evento, FechaUtc DESC);
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_Insert
(
    @Codigo VARCHAR(20), @PayloadJson NVARCHAR(MAX), @FechaCreacionUtc DATETIME2(3),
    @FechaExpiracionUtc DATETIME2(3), @ClienteId NVARCHAR(100) = NULL,
    @CreadoPor NVARCHAR(100) = NULL, @MaxDescargas SMALLINT = 5,
    @VentanaDescargaMinutos SMALLINT = 60, @Id BIGINT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ReporteLink
    (Codigo, PayloadJson, FechaCreacionUtc, FechaExpiracionUtc, Usado, FechaUsoUtc,
     ClienteId, CreadoPor, MaxDescargas, CantidadDescargas, VentanaDescargaMinutos,
     FechaPrimerIntentoUtc, FechaVentanaHastaUtc, FechaUltimaDescargaUtc)
    VALUES
    (@Codigo, @PayloadJson, @FechaCreacionUtc, @FechaExpiracionUtc, 0, NULL,
     @ClienteId, @CreadoPor, CASE WHEN @MaxDescargas < 1 THEN 1 ELSE @MaxDescargas END, 0,
     CASE WHEN @VentanaDescargaMinutos < 1 THEN 1 ELSE @VentanaDescargaMinutos END,
     NULL, NULL, NULL);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_ResuelveTodoEnUno
(
    @Codigo VARCHAR(20), @FechaActualUtc DATETIME2(3) = NULL,
    @Ip VARCHAR(45) = NULL, @UserAgent NVARCHAR(500) = NULL, @Referer NVARCHAR(1000) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @FechaActualUtc IS NULL SET @FechaActualUtc = SYSUTCDATETIME();
    BEGIN TRANSACTION;

    DECLARE @Id BIGINT, @PayloadJson NVARCHAR(MAX), @FechaCreacionUtc DATETIME2(3),
        @FechaExpiracionUtc DATETIME2(3), @Usado BIT, @FechaUsoUtc DATETIME2(3),
        @ClienteId NVARCHAR(100), @CreadoPor NVARCHAR(100), @MaxDescargas SMALLINT,
        @CantidadDescargas SMALLINT, @VentanaDescargaMinutos SMALLINT,
        @FechaPrimerIntentoUtc DATETIME2(3), @FechaVentanaHastaUtc DATETIME2(3),
        @FechaUltimaDescargaUtc DATETIME2(3), @AccesoId BIGINT,
        @Estado INT = 0, @Evento VARCHAR(40) = 'INTENTO_ACEPTADO';

    SELECT @Id=Id, @PayloadJson=PayloadJson, @FechaCreacionUtc=FechaCreacionUtc,
        @FechaExpiracionUtc=FechaExpiracionUtc, @Usado=Usado, @FechaUsoUtc=FechaUsoUtc,
        @ClienteId=ClienteId, @CreadoPor=CreadoPor, @MaxDescargas=MaxDescargas,
        @CantidadDescargas=CantidadDescargas, @VentanaDescargaMinutos=VentanaDescargaMinutos,
        @FechaPrimerIntentoUtc=FechaPrimerIntentoUtc, @FechaVentanaHastaUtc=FechaVentanaHastaUtc,
        @FechaUltimaDescargaUtc=FechaUltimaDescargaUtc
    FROM dbo.ReporteLink WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE Codigo=@Codigo;

    IF @Id IS NULL
        SELECT @Estado=1, @Evento='RECHAZADO_NO_EXISTE';
    ELSE IF @FechaExpiracionUtc < @FechaActualUtc
        SELECT @Estado=3, @Evento='RECHAZADO_EXPIRADO';
    ELSE IF @CantidadDescargas >= @MaxDescargas
        SELECT @Estado=5, @Evento='RECHAZADO_LIMITE';
    ELSE
    BEGIN
        IF @FechaPrimerIntentoUtc IS NULL
        BEGIN
            SET @FechaPrimerIntentoUtc=@FechaActualUtc;
            SET @FechaVentanaHastaUtc=DATEADD(MINUTE,@VentanaDescargaMinutos,@FechaActualUtc);
            IF @FechaVentanaHastaUtc>@FechaExpiracionUtc SET @FechaVentanaHastaUtc=@FechaExpiracionUtc;
            UPDATE dbo.ReporteLink SET FechaPrimerIntentoUtc=@FechaPrimerIntentoUtc,
                FechaVentanaHastaUtc=@FechaVentanaHastaUtc WHERE Id=@Id;
        END
        ELSE IF @FechaVentanaHastaUtc < @FechaActualUtc
            SELECT @Estado=4, @Evento='RECHAZADO_VENTANA';
    END

    INSERT dbo.ReporteLinkAuditoria(ReporteLinkId,Codigo,FechaUtc,Evento,Ip,UserAgent,Referer)
    VALUES(@Id,@Codigo,@FechaActualUtc,@Evento,@Ip,@UserAgent,@Referer);
    SET @AccesoId=SCOPE_IDENTITY();
    COMMIT TRANSACTION;

    SELECT @Estado Estado, @Id Id, @Codigo Codigo,
        CASE WHEN @Estado=0 THEN @PayloadJson ELSE NULL END PayloadJson,
        @FechaCreacionUtc FechaCreacionUtc, @FechaExpiracionUtc FechaExpiracionUtc,
        @Usado Usado, @FechaUsoUtc FechaUsoUtc, @ClienteId ClienteId, @CreadoPor CreadoPor,
        @AccesoId AccesoId, ISNULL(@MaxDescargas,0) MaxDescargas,
        ISNULL(@CantidadDescargas,0) CantidadDescargas,
        @FechaPrimerIntentoUtc FechaPrimerIntentoUtc,
        @FechaUltimaDescargaUtc FechaUltimaDescargaUtc,
        @FechaVentanaHastaUtc FechaVentanaHastaUtc;
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_ConfirmarDescarga
(
    @Codigo VARCHAR(20), @AccesoId BIGINT, @Bytes BIGINT = NULL,
    @DuracionMs INT = NULL, @ResultadoHttp SMALLINT = 200,
    @FechaActualUtc DATETIME2(3) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @FechaActualUtc IS NULL SET @FechaActualUtc=SYSUTCDATETIME();
    BEGIN TRANSACTION;
    DECLARE @Id BIGINT, @Cantidad SMALLINT, @Max SMALLINT, @Evento VARCHAR(40);
    SELECT @Id=Id,@Cantidad=CantidadDescargas,@Max=MaxDescargas
    FROM dbo.ReporteLink WITH (UPDLOCK,HOLDLOCK,ROWLOCK) WHERE Codigo=@Codigo;
    SELECT @Evento=Evento FROM dbo.ReporteLinkAuditoria WITH (UPDLOCK,HOLDLOCK,ROWLOCK)
    WHERE Id=@AccesoId AND Codigo=@Codigo;

    IF @Evento='DESCARGA_OK'
    BEGIN
        COMMIT TRANSACTION;
        SELECT 0 Estado,'La descarga ya estaba confirmada.' Mensaje,
            ISNULL(@Cantidad,0) CantidadDescargas,ISNULL(@Max,0) MaxDescargas;
        RETURN;
    END
    IF @Id IS NULL OR @Evento<>'INTENTO_ACEPTADO'
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 1 Estado,'El intento de descarga no es válido.' Mensaje,
            ISNULL(@Cantidad,0) CantidadDescargas,ISNULL(@Max,0) MaxDescargas;
        RETURN;
    END
    IF @Cantidad>=@Max
    BEGIN
        UPDATE dbo.ReporteLinkAuditoria SET Evento='RECHAZADO_LIMITE',ResultadoHttp=405,
            DuracionMs=@DuracionMs,Detalle='Límite alcanzado durante la confirmación.' WHERE Id=@AccesoId;
        COMMIT TRANSACTION;
        SELECT 5 Estado,'El enlace alcanzó el límite de descargas permitido.' Mensaje,
            @Cantidad CantidadDescargas,@Max MaxDescargas;
        RETURN;
    END

    SET @Cantidad+=1;
    UPDATE dbo.ReporteLink SET CantidadDescargas=@Cantidad,Usado=1,
        FechaUsoUtc=COALESCE(FechaUsoUtc,@FechaActualUtc),FechaUltimaDescargaUtc=@FechaActualUtc WHERE Id=@Id;
    UPDATE dbo.ReporteLinkAuditoria SET Evento='DESCARGA_OK',ResultadoHttp=@ResultadoHttp,
        Bytes=@Bytes,DuracionMs=@DuracionMs WHERE Id=@AccesoId;
    COMMIT TRANSACTION;
    SELECT 0 Estado,'Descarga confirmada.' Mensaje,@Cantidad CantidadDescargas,@Max MaxDescargas;
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_RegistrarFallo
(@Codigo VARCHAR(20),@AccesoId BIGINT,@DuracionMs INT=NULL,@ResultadoHttp SMALLINT=500,@Detalle NVARCHAR(500)=NULL)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ReporteLinkAuditoria SET Evento='GENERACION_FALLIDA',ResultadoHttp=@ResultadoHttp,
        DuracionMs=@DuracionMs,Detalle=@Detalle
    WHERE Id=@AccesoId AND Codigo=@Codigo AND Evento='INTENTO_ACEPTADO';
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_AuditoriaResumen
(@DesdeUtc DATETIME2(3),@HastaUtc DATETIME2(3))
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) EnlacesGenerados,
        SUM(CASE WHEN CantidadDescargas>0 THEN 1 ELSE 0 END) EnlacesDescargados,
        SUM(CASE WHEN CantidadDescargas=0 THEN 1 ELSE 0 END) EnlacesSinDescarga,
        SUM(CantidadDescargas) DescargasCompletadas,
        CAST(AVG(CAST(CantidadDescargas AS DECIMAL(10,2))) AS DECIMAL(10,2)) PromedioDescargasPorEnlace,
        SUM(CASE WHEN CantidadDescargas>=MaxDescargas THEN 1 ELSE 0 END) EnlacesQueAlcanzaronLimite
    FROM dbo.ReporteLink WHERE FechaCreacionUtc>=@DesdeUtc AND FechaCreacionUtc<@HastaUtc;

    SELECT CantidadDescargas,COUNT(*) CantidadEnlaces,
        CAST(100.0*COUNT(*)/NULLIF(SUM(COUNT(*)) OVER(),0) AS DECIMAL(6,2)) Porcentaje
    FROM dbo.ReporteLink WHERE FechaCreacionUtc>=@DesdeUtc AND FechaCreacionUtc<@HastaUtc
    GROUP BY CantidadDescargas ORDER BY CantidadDescargas;

    SELECT Evento,COUNT(*) Cantidad FROM dbo.ReporteLinkAuditoria
    WHERE FechaUtc>=@DesdeUtc AND FechaUtc<@HastaUtc GROUP BY Evento ORDER BY Evento;
END
GO

CREATE OR ALTER PROCEDURE dbo.SPGECO_REPO_ReporteLink_AuditoriaEliminarAntigua
(@ConservarDias INT=180,@FechaActualUtc DATETIME2(3)=NULL)
AS
BEGIN
    SET NOCOUNT ON;
    IF @FechaActualUtc IS NULL SET @FechaActualUtc=SYSUTCDATETIME();
    IF @ConservarDias<30 SET @ConservarDias=30;
    DELETE dbo.ReporteLinkAuditoria WHERE FechaUtc<DATEADD(DAY,-@ConservarDias,@FechaActualUtc);
END
GO
