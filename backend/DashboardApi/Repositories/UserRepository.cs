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
                @"SELECT id, username, display_name, password_hash
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
            PasswordHash = reader.GetString(3)
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
}