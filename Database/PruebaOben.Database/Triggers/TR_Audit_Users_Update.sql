CREATE TRIGGER [dbo].[TR_Audit_Users_Update]
ON [dbo].[users]
AFTER UPDATE
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

        CASE
            WHEN d.[deletedAt] IS NULL
                 AND i.[deletedAt] IS NOT NULL
            THEN N'DELETE'
            ELSE N'UPDATE'
        END,

        N'users',
        i.[id],
        cambios.[nombreCampo],
        cambios.[valorAnterior],
        cambios.[valorNuevo],

        TRY_CONVERT(
            INT,
            SESSION_CONTEXT(N'UserId')
        )

    FROM inserted AS i

    INNER JOIN deleted AS d
        ON d.[id] = i.[id]

    CROSS APPLY
    (
        VALUES
            (
                N'username',
                CONVERT(NVARCHAR(MAX), d.[username]),
                CONVERT(NVARCHAR(MAX), i.[username])
            ),
            (
                N'fullname',
                CONVERT(NVARCHAR(MAX), d.[fullname]),
                CONVERT(NVARCHAR(MAX), i.[fullname])
            ),
            (
                N'email',
                CONVERT(NVARCHAR(MAX), d.[email]),
                CONVERT(NVARCHAR(MAX), i.[email])
            ),
            (
                N'rol',
                CONVERT(NVARCHAR(MAX), d.[rol]),
                CONVERT(NVARCHAR(MAX), i.[rol])
            ),
            (
                N'active',
                CONVERT(NVARCHAR(MAX), d.[active]),
                CONVERT(NVARCHAR(MAX), i.[active])
            ),
            (
                N'deletedAt',
                CONVERT(NVARCHAR(MAX), d.[deletedAt]),
                CONVERT(NVARCHAR(MAX), i.[deletedAt])
            )
    ) AS cambios
    (
        [nombreCampo],
        [valorAnterior],
        [valorNuevo]
    )

    WHERE
        ISNULL(cambios.[valorAnterior], N'')
        <> ISNULL(cambios.[valorNuevo], N'');
END;