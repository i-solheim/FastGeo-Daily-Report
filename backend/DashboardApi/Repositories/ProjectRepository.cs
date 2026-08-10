using DashboardApi.Models;
using Npgsql;

namespace DashboardApi.Repositories;

public class ProjectRepository
{
    private readonly string _connectionString;

    public ProjectRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")!;
    }

    public async Task<ProjectMembership?> GetMembership(
        string projectKey,
        int userId)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
                SELECT
                    pm.project_id,
                    pm.user_id,
                    pm.role
                FROM project_members pm
                JOIN projects p
                    ON p.id = pm.project_id
                WHERE
                    p.project_key = @projectKey
                    AND pm.user_id = @userId;
                """,
                conn);

        cmd.Parameters.AddWithValue("projectKey", projectKey);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ProjectMembership
        {
            ProjectId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            Role = reader.GetString(2)
        };
    }
}