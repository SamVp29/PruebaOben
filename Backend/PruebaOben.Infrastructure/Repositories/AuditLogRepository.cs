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
        int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string countSql = """
        SELECT COUNT_BIG(*)
        FROM dbo.auditLogs;
        """;

        using var countCommand = new SqlCommand(countSql, connection);
        var totalCount = (long)(await countCommand.ExecuteScalarAsync()
            ?? throw new InvalidOperationException(
                "No se pudo obtener el total de registros de auditoría."));

        const string pageSql = """
        SELECT
            id,
            userId,
            accion,
            entidad,
            entidadId,
            nombreCampo,
            valorAnterior,
            valorNuevo,
            cambioRealizado,
            cambioAt
        FROM dbo.auditLogs
        ORDER BY cambioAt DESC, id DESC
        OFFSET @offset ROWS
        FETCH NEXT @pageSize ROWS ONLY;
        """;

        using var pageCommand = new SqlCommand(pageSql, connection);
        pageCommand.Parameters.Add("@offset", SqlDbType.BigInt).Value = offset;
        pageCommand.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;

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
                cambioAt = reader.GetDateTime(reader.GetOrdinal("cambioAt"))
            });
        }

        return (items, totalCount);
    }
}
