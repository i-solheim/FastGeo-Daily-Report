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

    public async Task<AddProjectMemberResult> AddMember(
    string projectKey,
    int userId,
    string role)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        // Find project
        await using var projectCmd =
            new NpgsqlCommand(
                """
            SELECT id
            FROM projects
            WHERE project_key = @projectKey;
            """,
                conn);

        projectCmd.Parameters.AddWithValue(
            "projectKey",
            projectKey);

        var projectResult =
            await projectCmd.ExecuteScalarAsync();

        if (projectResult == null)
        {
            return AddProjectMemberResult.ProjectNotFound;
        }

        var projectId = (int)projectResult;

        // Make sure user exists
        await using var userCmd =
            new NpgsqlCommand(
                """
            SELECT 1
            FROM users
            WHERE id = @userId;
            """,
                conn);

        userCmd.Parameters.AddWithValue("userId", userId);

        var userResult =
            await userCmd.ExecuteScalarAsync();

        if (userResult == null)
        {
            return AddProjectMemberResult.UserNotFound;
        }

        // Check existing membership
        await using var membershipCmd =
            new NpgsqlCommand(
                """
            SELECT 1
            FROM project_members
            WHERE project_id = @projectId
              AND user_id = @userId;
            """,
                conn);

        membershipCmd.Parameters.AddWithValue(
            "projectId",
            projectId);

        membershipCmd.Parameters.AddWithValue(
            "userId",
            userId);

        var membershipResult =
            await membershipCmd.ExecuteScalarAsync();

        if (membershipResult != null)
        {
            return AddProjectMemberResult.AlreadyMember;
        }

        // Create membership
        await using var insertCmd =
            new NpgsqlCommand(
                """
            INSERT INTO project_members
                (project_id, user_id, role)
            VALUES
                (@projectId, @userId, @role);
            """,
                conn);

        insertCmd.Parameters.AddWithValue(
            "projectId",
            projectId);

        insertCmd.Parameters.AddWithValue(
            "userId",
            userId);

        insertCmd.Parameters.AddWithValue(
            "role",
            role);

        await insertCmd.ExecuteNonQueryAsync();

        return AddProjectMemberResult.Added;
    }

    public async Task<List<ProjectMembership>?> GetMembers(
    string projectKey)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
            SELECT
                p.id,
                pm.user_id,
                pm.role
            FROM projects p
            LEFT JOIN project_members pm
                ON pm.project_id = p.id
            WHERE p.project_key = @projectKey
            ORDER BY pm.role, pm.user_id;
            """,
                conn);

        cmd.Parameters.AddWithValue("projectKey", projectKey);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            // Project doesn't exist
            return null;
        }

        var members = new List<ProjectMembership>();

        do
        {
            // LEFT JOIN means pm may be NULL
            if (!reader.IsDBNull(1))
            {
                members.Add(new ProjectMembership
                {
                    ProjectId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Role = reader.GetString(2)
                });
            }
        }
        while (await reader.ReadAsync());

        return members;
    }

    public async Task<bool> UpdateRole(
    string projectKey,
    int userId,
    string role)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                """
            UPDATE project_members pm
            SET role = @role
            FROM projects p
            WHERE
                pm.project_id = p.id
                AND p.project_key = @projectKey
                AND pm.user_id = @userId;
            """,
                conn);

        cmd.Parameters.AddWithValue("projectKey", projectKey);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("role", role);

        var rowsAffected =
            await cmd.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<RemoveProjectMemberResult> RemoveMember(
    string projectKey,
    int userId)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        // Find the membership and its role
        await using var membershipCmd =
            new NpgsqlCommand(
                """
            SELECT pm.role
            FROM project_members pm
            JOIN projects p
                ON p.id = pm.project_id
            WHERE p.project_key = @projectKey
              AND pm.user_id = @userId;
            """,
                conn);

        membershipCmd.Parameters.AddWithValue(
            "projectKey",
            projectKey);

        membershipCmd.Parameters.AddWithValue(
            "userId",
            userId);

        var roleResult =
            await membershipCmd.ExecuteScalarAsync();

        if (roleResult == null)
        {
            return RemoveProjectMemberResult.NotFound;
        }

        var role = (string)roleResult;

        // If this is a Leader, make sure another Leader remains
        if (role == "Leader")
        {
            await using var leaderCmd =
                new NpgsqlCommand(
                    """
                SELECT COUNT(*)
                FROM project_members pm
                JOIN projects p
                    ON p.id = pm.project_id
                WHERE p.project_key = @projectKey
                  AND pm.role = 'Leader'
                  AND pm.user_id <> @userId;
                """,
                    conn);

            leaderCmd.Parameters.AddWithValue(
                "projectKey",
                projectKey);

            leaderCmd.Parameters.AddWithValue(
                "userId",
                userId);

            var otherLeaderCount =
                (long)(await leaderCmd.ExecuteScalarAsync())!;

            if (otherLeaderCount == 0)
            {
                return RemoveProjectMemberResult.LastLeader;
            }
        }

        // Remove membership
        await using var deleteCmd =
            new NpgsqlCommand(
                """
            DELETE FROM project_members pm
            USING projects p
            WHERE pm.project_id = p.id
              AND p.project_key = @projectKey
              AND pm.user_id = @userId;
            """,
                conn);

        deleteCmd.Parameters.AddWithValue(
            "projectKey",
            projectKey);

        deleteCmd.Parameters.AddWithValue(
            "userId",
            userId);

        await deleteCmd.ExecuteNonQueryAsync();

        return RemoveProjectMemberResult.Removed;
    }
}