USE [GeConnectDB]
GO
/****** Object:  Table [dbo].[Acceso]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Acceso](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [uniqueidentifier] NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[IP] [char](15) NOT NULL,
	[TipoAcceso] [char](1) NOT NULL,
 CONSTRAINT [PK_Accesos_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuditoriaUsuario]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditoriaUsuario](
	[Id] [int] NOT NULL,
	[UsuarioId] [uniqueidentifier] NOT NULL,
	[FechaAuditoria] [datetime] NOT NULL,
	[IP] [char](15) NOT NULL,
	[MetodoAccedido] [varchar](100) NOT NULL,
 CONSTRAINT [PK_AuditoriaUsuario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Autorizado]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Autorizado](
	[UsuarioId] [uniqueidentifier] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Autorizado] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[UsuarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rol]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol](
	[Id] [uniqueidentifier] NOT NULL,
	[Nombre] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Rol] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Test]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Test](
	[Id] [int] NOT NULL,
	[DatoInt] [int] NULL,
	[DatoStr] [varchar](50) NOT NULL,
	[DatoBool] [bit] NOT NULL,
 CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoDocumento]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoDocumento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descripcion] [varchar](10) NOT NULL,
	[Hasar] [varchar](2) NULL,
	[Epson] [varchar](4) NULL,
 CONSTRAINT [PK_TipoDocumento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Usuario]    Script Date: 22/2/2024 18:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[Id] [uniqueidentifier] NOT NULL,
	[Contrasena] [varchar](200) NOT NULL,
	[Correo] [varchar](50) NOT NULL,
	[Bloqueado] [bit] NOT NULL,
	[Intentos] [int] NOT NULL,
	[FechaAlta] [datetime] NOT NULL,
	[FechaBloqueo] [datetime] NULL,
	[UserName] [varchar](50) NOT NULL,
	[EstaLogueado] [bit] NOT NULL,
 CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Acceso] ON 

INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10053, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-18T11:38:25.257' AS DateTime), N'192.168.0.101  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10054, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:00:16.147' AS DateTime), N'192.168.0.104  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10055, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:01:34.637' AS DateTime), N'192.168.0.104  ', N'O')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10056, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:01:46.170' AS DateTime), N'192.168.0.104  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10057, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:02:03.910' AS DateTime), N'192.168.0.104  ', N'O')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10058, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:02:46.540' AS DateTime), N'192.168.0.104  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10059, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-05-25T01:06:23.037' AS DateTime), N'192.168.0.104  ', N'O')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10060, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-05-25T01:06:36.237' AS DateTime), N'192.168.0.104  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10061, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-05-25T01:11:04.787' AS DateTime), N'192.168.0.104  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10062, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-05-29T23:43:43.933' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10063, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-05-30T16:42:32.260' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (10064, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-05-31T13:27:24.240' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20055, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-06-04T00:33:23.353' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20056, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-06-04T09:52:35.370' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20057, N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', CAST(N'2022-06-04T10:40:08.740' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20059, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T10:42:19.943' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20060, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T10:55:20.143' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20061, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T11:10:43.653' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20062, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T15:05:59.507' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20063, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T15:36:22.600' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20064, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T20:07:07.907' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20065, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T20:17:36.640' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20066, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T20:30:34.257' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20067, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T20:40:40.953' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20068, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-04T20:52:55.653' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20069, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-05T09:12:03.863' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20070, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-06-05T22:02:33.637' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (20071, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-06-30T20:08:59.510' AS DateTime), N'10.212.133.6   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (30071, N'fcf9e71b-7841-4b25-bf58-9b95896278f1', CAST(N'2022-07-06T22:46:53.727' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40071, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-07-28T22:38:50.147' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40073, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-07-29T00:18:45.310' AS DateTime), N'192.168.0.100  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40074, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-07-29T22:58:45.280' AS DateTime), N'192.168.0.102  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40075, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-02T18:58:45.503' AS DateTime), N'192.168.0.102  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40076, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-03T15:43:32.113' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40078, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-03T15:43:47.777' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40080, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-03T16:20:22.120' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (40081, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-03T16:20:38.537' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50071, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-08T17:19:11.647' AS DateTime), N'10.212.133.1   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50072, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-10T00:06:04.070' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50073, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-16T16:29:42.927' AS DateTime), N'10.212.133.1   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50074, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-18T00:16:20.390' AS DateTime), N'192.168.0.105  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50075, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-19T10:47:31.700' AS DateTime), N'10.212.133.8   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50076, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-20T23:35:06.960' AS DateTime), N'192.168.0.105  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50077, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-25T00:30:37.033' AS DateTime), N'192.168.0.105  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50078, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-26T22:05:06.720' AS DateTime), N'192.168.0.105  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50079, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-08-27T22:06:05.663' AS DateTime), N'192.168.0.105  ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (50080, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-09-05T13:52:55.420' AS DateTime), N'172.23.246.101 ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (60080, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-09-12T20:51:09.283' AS DateTime), N'10.212.133.2   ', N'L')
INSERT [dbo].[Acceso] ([Id], [UsuarioId], [Fecha], [IP], [TipoAcceso]) VALUES (60081, N'051ff297-672e-4800-ab65-16068fb681e9', CAST(N'2022-09-13T18:43:17.233' AS DateTime), N'10.212.133.1   ', N'L')
SET IDENTITY_INSERT [dbo].[Acceso] OFF
GO
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'051ff297-672e-4800-ab65-16068fb681e9', N'cc4cb40a-f61e-42b0-958e-4a281a31a10f')
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'e0afaa3b-c601-4a29-ad3a-845f5eef9235', N'cc4cb40a-f61e-42b0-958e-4a281a31a10f')
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'fcf9e71b-7841-4b25-bf58-9b95896278f1', N'cc4cb40a-f61e-42b0-958e-4a281a31a10f')
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', N'c1954901-0e29-44da-a9f5-afdba29b686d')
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'bab1888d-472a-428b-9bc4-52c620ce5433', N'c1954901-0e29-44da-a9f5-afdba29b686d')
INSERT [dbo].[Autorizado] ([UsuarioId], [RoleId]) VALUES (N'89823ed3-2d36-4b8e-8297-f5bd3bfffd88', N'c1954901-0e29-44da-a9f5-afdba29b686d')
GO
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'cc4cb40a-f61e-42b0-958e-4a281a31a10f', N'VENDEDOR')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'c1954901-0e29-44da-a9f5-afdba29b686d', N'ADMINISTRADOR')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'a22157cb-ce6d-417e-b04f-b8c1c2482920', N'CAJERO')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'740b544d-d7a1-453c-83f7-c965df5d0e1e', N'LABORATORISTA')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'6c52ea76-d51e-43c0-b2ce-dd52a6a6000b', N'ADMINISTRACION')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (N'08dbeeac-2d7f-46a4-9ef0-f758eb81a0fe', N'CONSULTA')
GO
SET IDENTITY_INSERT [dbo].[TipoDocumento] ON 

INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (1, N'CUIT', N'67', N'CUIT')
INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (2, N'LC', N'48', N'LC')
INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (3, N'LE', N'49', N'LE')
INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (4, N'DNI', N'50', N'DNI')
INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (5, N'PASAPORTE', N'51', N'PRTE')
INSERT [dbo].[TipoDocumento] ([Id], [Descripcion], [Hasar], [Epson]) VALUES (6, N'CI', N'52', N'CI')
SET IDENTITY_INSERT [dbo].[TipoDocumento] OFF
GO
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'051ff297-672e-4800-ab65-16068fb681e9', N'10000.FEq3GW02V/xpUGIRRYlFXQ==.gEuOzbCfY13LmPZ7D60j0DEDmaBFiEQjjCDyZqnVHZA=', N'juanjobe@gmail.com', 0, 0, CAST(N'2022-06-04T10:41:55.207' AS DateTime), NULL, N'juanma', 0)
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'3d2bc71c-e9f9-4b78-ad04-2cc867aa32cf', N'10000.81FqWDjaSyth/SLSDd9cxA==.kv8J5jrXD+UA2dR9CW4dYYjN6VoMo+Wm+T29xRLnKiI=', N'juanjobe@gmail.com', 0, 0, CAST(N'2021-05-16T09:11:23.077' AS DateTime), NULL, N'superusuario', 0)
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'bab1888d-472a-428b-9bc4-52c620ce5433', N'10000.xJsFc/jh4wR60YFQNt1l8g==.7xHqzk+qkcO/xVFFIyjblgQH1gfginNK0X/pwzHn6+U=', N'admin@gmail.com', 0, 0, CAST(N'2022-05-18T16:33:24.967' AS DateTime), NULL, N'orlando', 0)
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'e0afaa3b-c601-4a29-ad3a-845f5eef9235', N'10000.TCQ6TH5TXiNUMWcuelBOSw==.UZCeatjAXEdV/mfD6qWFRd2Oa4s9zOnRsK0sZIpGtPs=', N'juanmanuel@gmail.com', 0, 0, CAST(N'2024-02-22T17:14:08.970' AS DateTime), NULL, N'gcjuanma', 0)
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'fcf9e71b-7841-4b25-bf58-9b95896278f1', N'10000.XyI3AHtdm4XOsjeG/1IRPQ==.PF0kCrzZYDRwSr2sFrqjJYYcL5M4drzIF0T0FtdDaO8=', N'juanjobe@gmail.com', 0, 0, CAST(N'2022-05-25T01:06:12.470' AS DateTime), NULL, N'juanjobe', 0)
INSERT [dbo].[Usuario] ([Id], [Contrasena], [Correo], [Bloqueado], [Intentos], [FechaAlta], [FechaBloqueo], [UserName], [EstaLogueado]) VALUES (N'89823ed3-2d36-4b8e-8297-f5bd3bfffd88', N'10000.TCQ6TH5TXiNUMWcuelBOSw==.w3VkE1xxeaABVteSEOR2AT2VHY78wsK5q35y4duk07s=', N'juanjobe@gmail.com', 0, 0, CAST(N'2024-02-21T20:29:05.130' AS DateTime), NULL, N'gcjuanjobe', 0)
GO
ALTER TABLE [dbo].[Usuario] ADD  CONSTRAINT [DF__Usuario__User__1EA48E88]  DEFAULT ('') FOR [UserName]
GO
ALTER TABLE [dbo].[Acceso]  WITH CHECK ADD  CONSTRAINT [FK_Accesos_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Acceso] CHECK CONSTRAINT [FK_Accesos_Usuario]
GO
ALTER TABLE [dbo].[AuditoriaUsuario]  WITH CHECK ADD  CONSTRAINT [FK_AuditoriaUsuario_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[AuditoriaUsuario] CHECK CONSTRAINT [FK_AuditoriaUsuario_Usuario]
GO
ALTER TABLE [dbo].[Autorizado]  WITH CHECK ADD  CONSTRAINT [FK_Autorizado_Rol] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Rol] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[Autorizado] CHECK CONSTRAINT [FK_Autorizado_Rol]
GO
ALTER TABLE [dbo].[Autorizado]  WITH CHECK ADD  CONSTRAINT [FK_Autorizado_Usuario] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Autorizado] CHECK CONSTRAINT [FK_Autorizado_Usuario]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Podrá ser "L" de Login y "R" de Relogin cuando el usuario ya se encuentre logueado.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Acceso', @level2type=N'COLUMN',@level2name=N'TipoAcceso'
GO
