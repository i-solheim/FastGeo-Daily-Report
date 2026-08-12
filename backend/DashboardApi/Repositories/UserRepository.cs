using DashboardApi.Models;
using Npgsql;

namespace DashboardApi.Repositories;

public class UserRepository
{
    private readonly string _connectionString;
    public UserRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")!;
    }

    public async Task<User?> GetByUsername(string username)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                @"SELECT id, username, display_name, is_admin
                FROM users
                WHERE username = @username;",
                conn);

        cmd.Parameters.AddWithValue("username", username);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            DisplayName = reader.IsDBNull(2)
                ? ""
                : reader.GetString(2),
            IsAdmin = reader.GetBoolean(3)
        };
    }

    public async Task UpdateDisplayName(
    int userId,
    string displayName)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
            UPDATE users
            SET display_name = @displayName
            WHERE id = @id;
            """,
                conn);

        cmd.Parameters.AddWithValue("id", userId);
        cmd.Parameters.AddWithValue("displayName", displayName);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<User> CreateUser(
    string username,
    string displayName)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
            INSERT INTO users
                (username, display_name)
            VALUES
                (@username, @displayName)
            RETURNING id;
            """,
                conn);

        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("displayName", displayName);

        var id = (int)(await cmd.ExecuteScalarAsync())!;

        return new User
        {
            Id = id,
            Username = username,
            DisplayName = displayName,
            IsAdmin = false
        };
    }

    public async Task<List<User>> GetUsers()
    {
        var users = new List<User>();

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
            SELECT id, username, display_name, is_admin
            FROM users
            ORDER BY username;
            """,
                conn);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                DisplayName = reader.IsDBNull(2)
                    ? ""
                    : reader.GetString(2),
                IsAdmin = reader.GetBoolean(3)
            });
        }

        return users;
    }
}