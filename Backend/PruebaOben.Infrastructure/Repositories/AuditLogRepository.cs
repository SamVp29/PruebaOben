using Microsoft.Data.SqlClient;
using PruebaOben.Domain.Entities;
using PruebaOben.Domain.Interfaces;
using PruebaOben.Infrastructure.Data;
using System.Data;

namespace PruebaOben.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public AuditLogRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(IReadOnlyList<AuditLog> Items, long TotalCount)> GetPageAsync(
        long offset,
        int pageSize,
        string? action)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string countSql = """
        SELECT COUNT_BIG(*)
        FROM dbo.auditLogs
        WHERE @action IS NULL OR accion = @action;
        """;

        using var countCommand = new SqlCommand(countSql, connection);
        countCommand.Parameters.Add("@action", SqlDbType.NVarChar, 20).Value =
            (object?)action ?? DBNull.Value;
        var totalCount = (long)(await countCommand.ExecuteScalarAsync()
            ?? throw new InvalidOperationException(
                "No se pudo obtener el total de registros de auditoría."));

        const string pageSql = """
        SELECT
            audit.id,
            audit.userId,
            COALESCE(
                affectedUser.username,
                affectedDelete.username
            ) AS affectedUsername,
            audit.accion,
            audit.entidad,
            audit.entidadId,
            audit.nombreCampo,
            audit.valorAnterior,
            audit.valorNuevo,
            audit.cambioRealizado,
            COALESCE(
                actor.username,
                actorDelete.username
            ) AS actorUsername,
            audit.cambioAt
        FROM dbo.auditLogs AS audit
        LEFT JOIN dbo.users AS affectedUser
            ON affectedUser.id = COALESCE(
                audit.userId,
                CASE WHEN audit.entidad = N'users' THEN audit.entidadId END
            )
        LEFT JOIN dbo.users AS actor
            ON actor.id = audit.cambioRealizado
        OUTER APPLY
        (
            SELECT TOP (1) deletedUser.valorAnterior AS username
            FROM dbo.auditLogs AS deletedUser
            WHERE deletedUser.entidad = N'users'
                AND deletedUser.entidadId = COALESCE(
                    audit.userId,
                    CASE WHEN audit.entidad = N'users' THEN audit.entidadId END
                )
                AND deletedUser.accion = N'DELETE'
                AND deletedUser.nombreCampo = N'username'
                AND deletedUser.valorAnterior IS NOT NULL
            ORDER BY deletedUser.cambioAt DESC, deletedUser.id DESC
        ) AS affectedDelete
        OUTER APPLY
        (
            SELECT TOP (1) deletedActor.valorAnterior AS username
            FROM dbo.auditLogs AS deletedActor
            WHERE deletedActor.entidad = N'users'
                AND deletedActor.entidadId = audit.cambioRealizado
                AND deletedActor.accion = N'DELETE'
                AND deletedActor.nombreCampo = N'username'
                AND deletedActor.valorAnterior IS NOT NULL
            ORDER BY deletedActor.cambioAt DESC, deletedActor.id DESC
        ) AS actorDelete
        WHERE @action IS NULL OR audit.accion = @action
        ORDER BY audit.cambioAt DESC, audit.id DESC
        OFFSET @offset ROWS
        FETCH NEXT @pageSize ROWS ONLY;
        """;

        using var pageCommand = new SqlCommand(pageSql, connection);
        pageCommand.Parameters.Add("@offset", SqlDbType.BigInt).Value = offset;
        pageCommand.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;
        pageCommand.Parameters.Add("@action", SqlDbType.NVarChar, 20).Value =
            (object?)action ?? DBNull.Value;

        var items = new List<AuditLog>();
        using var reader = await pageCommand.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AuditLog
            {
                id = reader.GetInt64(reader.GetOrdinal("id")),
                userId = reader.IsDBNull(reader.GetOrdinal("userId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("userId")),
                affectedUsername = reader.IsDBNull(reader.GetOrdinal("affectedUsername"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("affectedUsername")),
                accion = reader.GetString(reader.GetOrdinal("accion")),
                entidad = reader.GetString(reader.GetOrdinal("entidad")),
                entidadId = reader.IsDBNull(reader.GetOrdinal("entidadId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("entidadId")),
                nombreCampo = reader.IsDBNull(reader.GetOrdinal("nombreCampo"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("nombreCampo")),
                valorAnterior = reader.IsDBNull(reader.GetOrdinal("valorAnterior"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valorAnterior")),
                valorNuevo = reader.IsDBNull(reader.GetOrdinal("valorNuevo"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("valorNuevo")),
                cambioRealizado = reader.IsDBNull(reader.GetOrdinal("cambioRealizado"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("cambioRealizado")),
                actorUsername = reader.IsDBNull(reader.GetOrdinal("actorUsername"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("actorUsername")),
                cambioAt = reader.GetDateTime(reader.GetOrdinal("cambioAt"))
            });
        }

        return (items, totalCount);
    }
}
