using Microsoft.Data.SqlClient;
using PruebaOben.Domain.Entities;
using PruebaOben.Domain.Interfaces;
using PruebaOben.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace PruebaOben.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public UserRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = new List<User>();

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT
            id,
            username,
            fullname,
            email,
            passwordHash,
            rol,
            active,
            createdAt,
            UpdatedAt,
            deletedAt
        FROM dbo.users
        WHERE deletedAt IS NULL;
        """;

        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                username = reader.GetString(reader.GetOrdinal("username")),
                fullname = reader.GetString(reader.GetOrdinal("fullname")),
                email = reader.GetString(reader.GetOrdinal("email")),
                passwordHash = reader.GetString(reader.GetOrdinal("passwordHash")),
                rol = reader.GetString(reader.GetOrdinal("rol")),
                active = reader.GetBoolean(reader.GetOrdinal("active")),
                createdAt = reader.GetDateTime(reader.GetOrdinal("createdAt")),
                updatedAt = reader.IsDBNull(reader.GetOrdinal("updatedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("updatedAt")),
                deletedAt = reader.IsDBNull(reader.GetOrdinal("deletedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("deletedAt"))
            });
        }

        return users;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT
            id,
            username,
            fullname,
            email,
            passwordHash,
            rol,
            active,
            createdAt,
            UpdatedAt,
            deletedAt
        FROM dbo.users
        WHERE id = @id
          AND deletedAt IS NULL;
        """;

        using var command = new SqlCommand(sql, connection);

        // @id evita concatenar directamente el valor dentro del SQL.
        // Esto ayuda a prevenir SQL Injection.
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        // Si no existe el usuario, devolvemos null.
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new User
        {
            id = reader.GetInt32(reader.GetOrdinal("id")),
            username = reader.GetString(reader.GetOrdinal("username")),
            fullname = reader.GetString(reader.GetOrdinal("fullname")),
            email = reader.GetString(reader.GetOrdinal("email")),
            passwordHash = reader.GetString(reader.GetOrdinal("passwordHash")),
            rol = reader.GetString(reader.GetOrdinal("rol")),
            active = reader.GetBoolean(reader.GetOrdinal("active")),
            createdAt = reader.GetDateTime(reader.GetOrdinal("createdAt")),

            updatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

            deletedAt = reader.IsDBNull(reader.GetOrdinal("deletedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("deletedAt"))
        };
    }
    public async Task<User> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        INSERT INTO dbo.users
        (
            username,
            fullname,
            email,
            passwordHash,
            rol,
            active
        )
        OUTPUT
            INSERTED.id,
            INSERTED.username,
            INSERTED.fullname,
            INSERTED.email,
            INSERTED.passwordHash,
            INSERTED.rol,
            INSERTED.active,
            INSERTED.createdAt,
            INSERTED.UpdatedAt,
            INSERTED.deletedAt
        VALUES
        (
            @username,
            @fullname,
            @email,
            @passwordHash,
            @rol,
            @active
        );
        """;

        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = user.username;
        command.Parameters.Add("@fullname", SqlDbType.NVarChar, 150).Value = user.fullname;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = user.email;
        command.Parameters.Add("@passwordHash", SqlDbType.NVarChar, 255).Value = user.passwordHash;
        command.Parameters.Add("@rol", SqlDbType.NVarChar, 50).Value = user.rol;
        command.Parameters.Add("@active", SqlDbType.Bit).Value = user.active;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException(
                "No se pudo obtener el usuario creado.");
        }

        return new User
        {
            id = reader.GetInt32(reader.GetOrdinal("id")),
            username = reader.GetString(reader.GetOrdinal("username")),
            fullname = reader.GetString(reader.GetOrdinal("fullname")),
            email = reader.GetString(reader.GetOrdinal("email")),
            passwordHash = reader.GetString(reader.GetOrdinal("passwordHash")),
            rol = reader.GetString(reader.GetOrdinal("rol")),
            active = reader.GetBoolean(reader.GetOrdinal("active")),
            createdAt = reader.GetDateTime(reader.GetOrdinal("createdAt")),

            updatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

            deletedAt = reader.IsDBNull(reader.GetOrdinal("deletedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("deletedAt"))
        };
    }
    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        UPDATE dbo.users
        SET
            username = @username,
            fullname = @fullname,
            email = @email,
            rol = @rol,
            active = @active,
            UpdatedAt = SYSDATETIME()
        WHERE id = @id
          AND deletedAt IS NULL;
        """;

        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = user.id;
        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = user.username;
        command.Parameters.Add("@fullname", SqlDbType.NVarChar, 150).Value = user.fullname;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = user.email;
        command.Parameters.Add("@rol", SqlDbType.NVarChar, 50).Value = user.rol;
        command.Parameters.Add("@active", SqlDbType.Bit).Value = user.active;

        await connection.OpenAsync();

        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        UPDATE dbo.users
        SET
            active = 0,
            deletedAt = SYSDATETIME(),
            UpdatedAt = SYSDATETIME()
        WHERE id = @id
          AND deletedAt IS NULL;
        """;

        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
}

