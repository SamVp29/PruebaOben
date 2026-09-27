CREATE TRIGGER [dbo].[TR_Audit_Users_Delete]
ON [dbo].[users]
AFTER DELETE
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
        NULL,
        N'DELETE',
        N'users',
        d.[id],
        datosEliminados.[nombreCampo],
        datosEliminados.[valorAnterior],
        NULL,
        TRY_CONVERT(INT, SESSION_CONTEXT(N'UserId'))
    FROM deleted AS d
    CROSS APPLY
    (
        VALUES
            (N'id', CONVERT(NVARCHAR(MAX), d.[id])),
            (N'username', CONVERT(NVARCHAR(MAX), d.[username])),
            (N'fullname', CONVERT(NVARCHAR(MAX), d.[fullname]))
    ) AS datosEliminados ([nombreCampo], [valorAnterior]);
END;