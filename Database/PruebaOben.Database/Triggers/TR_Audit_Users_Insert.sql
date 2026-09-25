CREATE TRIGGER [dbo].[TR_Audit_Users_Insert]
ON [dbo].[users]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[auditLogs]
    (
        [userId],
        [accion],
        [entidad],
        [entidadId],
        [nombreCampo],
        [valorAnterior],
        [valorNuevo],
        [cambioRealizado]
    )
    SELECT
        i.[id],
        N'INSERT',
        N'users',
        i.[id],
        NULL,
        NULL,
        NULL,
        TRY_CONVERT(INT, SESSION_CONTEXT(N'UserId'))
    FROM inserted AS i;
END;