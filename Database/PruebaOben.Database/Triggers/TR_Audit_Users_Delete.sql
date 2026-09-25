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
        d.[id],                         -- Usuario eliminado
        N'DELETE',                     -- Acción
        N'users',                       -- Entidad
        d.[id],                         -- ID del usuario eliminado
        NULL,                           -- No corresponde a un campo específico
        NULL,                           -- No almacenamos los datos anteriores
        NULL,                           -- No hay valor nuevo
        TRY_CONVERT(
            INT,
            SESSION_CONTEXT(N'UserId')
        )                              -- Usuario que realizó la acción
    FROM deleted AS d;
END;