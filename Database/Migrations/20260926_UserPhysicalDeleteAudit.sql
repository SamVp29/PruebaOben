SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_AuditLogs_User'
      AND delete_referential_action_desc <> N'SET_NULL'
)
BEGIN
    ALTER TABLE dbo.auditLogs
        DROP CONSTRAINT FK_AuditLogs_User;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_AuditLogs_User'
      AND delete_referential_action_desc = N'SET_NULL'
)
BEGIN
    ALTER TABLE dbo.auditLogs
        ADD CONSTRAINT FK_AuditLogs_User
        FOREIGN KEY (userId)
        REFERENCES dbo.users(id)
        ON DELETE SET NULL;
END;

COMMIT TRANSACTION;
GO

CREATE OR ALTER TRIGGER dbo.TR_Audit_Users_Delete
ON dbo.users
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.auditLogs
    (
        userId,
        accion,
        entidad,
        entidadId,
        nombreCampo,
        valorAnterior,
        valorNuevo,
        cambioRealizado
    )
    SELECT
        NULL,
        N'DELETE',
        N'users',
        d.id,
        deletedFields.nombreCampo,
        deletedFields.valorAnterior,
        NULL,
        TRY_CONVERT(INT, SESSION_CONTEXT(N'UserId'))
    FROM deleted AS d
    CROSS APPLY
    (
        VALUES
            (N'id', CONVERT(NVARCHAR(MAX), d.id)),
            (N'username', CONVERT(NVARCHAR(MAX), d.username)),
            (N'fullname', CONVERT(NVARCHAR(MAX), d.fullname))
    ) AS deletedFields (nombreCampo, valorAnterior);
END;
GO
