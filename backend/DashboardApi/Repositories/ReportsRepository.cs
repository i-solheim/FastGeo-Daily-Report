using DashboardApi.Models;
using Npgsql;

namespace DashboardApi.Repositories;

public class DashboardRepository
{
    private readonly string _connectionString;

    public DashboardRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")!;
    }

    public async Task<List<ProjectAccessDto>> GetProjects(int userId)
    {
        var projects = new List<ProjectAccessDto>();

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                @"SELECT p.project_key, pm.role
                  FROM projects p
                  JOIN project_members pm
                      ON p.id = pm.project_id
                  WHERE pm.user_id = @userId
                  ORDER BY pm.project_id;",
                conn);

        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(new ProjectAccessDto
            {
                ProjectKey = reader.GetString(0),
                Role = reader.GetString(1)
            });
        }

        return projects;
    }

    public async Task<List<ChangeRecord>> GetChanges(
        string project,
        DateOnly day,
        string username,
        string role)
    {
        var changes = new List<ChangeRecord>();

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        var authorFilter =
            role == "Leader"
                ? ""
                : "AND sc.author = @username";

        var issueAuthorFilter =
            role == "Leader"
                ? ""
                : "AND i.author = @username";

        await using var cmd =
            new NpgsqlCommand(
            $@"SELECT *
            FROM
            (
                SELECT DISTINCT ON (sc.issue_key)
                    sc.issue_key,
                    i.issue_title,
                    i.issue_type,
                    i.issue_url,
                    COALESCE(u.display_name, sc.author) AS author,
                    sc.changed_at,
                    sc.from_status,
                    sc.to_status,
                    CASE
                        WHEN sc.to_status IN ('In review', 'Done')
                        THEN 'completed'
                        ELSE 'status_change'
                    END AS category

                FROM status_changes sc
                JOIN issues i
                    ON sc.issue_key = i.issue_key
                LEFT JOIN users u
                    ON sc.author = u.username

                WHERE
                    i.project = @project
                    AND sc.changed_at::date = @day
                    {authorFilter}

                ORDER BY
                    sc.issue_key,
                    sc.changed_at DESC
            ) latest_changes

            UNION ALL

            SELECT
                i.issue_key,
                i.issue_title,
                i.issue_type,
                i.issue_url,
                COALESCE(u.display_name, i.author) AS author,
                i.created_at,
                NULL,
                NULL,
                'new_task'

            FROM issues i
            LEFT JOIN users u
                ON i.author = u.username

            WHERE
                i.project = @project
                AND i.created_at::date = @day
                {issueAuthorFilter}

                -- only include issues that never changed status today
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM status_changes sc
                    WHERE
                        sc.issue_key = i.issue_key
                        AND sc.changed_at::date = @day
                )

            ORDER BY
                author,
                changed_at;",
            conn);

        cmd.Parameters.AddWithValue("project", project);
        cmd.Parameters.AddWithValue("day", day);

        if (role != "Leader")
        {
            cmd.Parameters.AddWithValue("username", username);
        }

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            changes.Add(new ChangeRecord
            {
                IssueKey = reader.GetString(0),
                IssueTitle = reader.GetString(1),
                IssueType = reader.GetString(2),
                IssueUrl = reader.GetString(3),
                Author = reader.GetString(4),
                ChangedAt = reader.GetDateTime(5),
                FromStatus = reader.IsDBNull(6) ? null : reader.GetString(6),
                ToStatus = reader.IsDBNull(7) ? null : reader.GetString(7),
                Category = reader.GetString(8)
            });
        }

        return changes;
    }
    public async Task<SummaryResult> GetSummary(
        string project,
        DateOnly day,
        string username,
        string role)
    {
        var counts = new Dictionary<string, int>
        {
            ["completed"] = 0,
            ["status_change"] = 0,
            ["new_task"] = 0
        };

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        var authorFilter =
            role == "Leader"
                ? ""
                : "AND sc.author = @username";

        var issueAuthorFilter =
            role == "Leader"
                ? ""
                : "AND i.author = @username";

        await using var cmd =
            new NpgsqlCommand(
            $@"SELECT category,
            COUNT(*)
            FROM
            (
                SELECT *
                FROM
                (
                    SELECT DISTINCT ON (sc.issue_key)
                        CASE
                            WHEN sc.to_status IN ('In review', 'Done')
                            THEN 'completed'
                            ELSE 'status_change'
                        END AS category
                    FROM status_changes sc
                    JOIN issues i
                        ON sc.issue_key = i.issue_key
                    WHERE
                        i.project = @project
                        AND sc.changed_at::date = @day
                        {authorFilter}
                    ORDER BY
                        sc.issue_key,
                        sc.changed_at DESC
                ) latest

                UNION ALL

                SELECT
                    'new_task'
                FROM issues i
                WHERE
                    i.project = @project
                    AND i.created_at::date = @day
                    {issueAuthorFilter}
                    AND NOT EXISTS
                    (
                        SELECT 1
                        FROM status_changes sc
                        WHERE
                            sc.issue_key = i.issue_key
                            AND sc.changed_at::date = @day
                    )
            ) combined

            GROUP BY category;",
            conn);

        cmd.Parameters.AddWithValue("project", project);
        cmd.Parameters.AddWithValue("day", day);

        if (role != "Leader")
        {
            cmd.Parameters.AddWithValue("username", username);
        }

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            counts[reader.GetString(0)] =
                reader.GetInt32(1);

        return new SummaryResult
        {
            Project = project,
            Date = day.ToString("yyyy-MM-dd"),
            Completed = counts["completed"],
            NewTasks = counts["new_task"],
            StatusChanges = counts["status_change"],
            Total =
                counts["completed"] +
                counts["new_task"] +
                counts["status_change"]
        };
    }
}