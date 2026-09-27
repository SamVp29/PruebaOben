CREATE TABLE [dbo].[auditLogs]
(
    [id] BIGINT IDENTITY(1,1) NOT NULL,

    -- Usuario afectado por el cambio.
    [userId] INT NULL,

    -- Tipo de operación realizada: INSERT, UPDATE o DELETE.
    [accion] NVARCHAR(20) NOT NULL,

    -- Tabla o entidad sobre la que ocurrió el cambio.
    [entidad] NVARCHAR(100) NOT NULL,

    -- ID del registro afectado.
    [entidadId] INT NULL,

    -- Nombre del campo que cambió.
    [nombreCampo] NVARCHAR(100) NULL,

    -- Valor que tenía el campo antes del cambio.
    [valorAnterior] NVARCHAR(MAX) NULL,

    -- Valor que tiene después del cambio.
    [valorNuevo] NVARCHAR(MAX) NULL,

    -- Usuario que realizó el cambio.
    [cambioRealizado] INT NULL,

    -- Fecha y hora del cambio.
    [cambioAt] DATETIME2 NOT NULL
        CONSTRAINT [DF_AuditLogs_ChangedAt] DEFAULT SYSDATETIME(),

    CONSTRAINT [PK_AuditLogs]
        PRIMARY KEY ([id]),

    CONSTRAINT [FK_AuditLogs_User]
        FOREIGN KEY ([userId])
        REFERENCES [dbo].[users]([id])
        ON DELETE SET NULL
);