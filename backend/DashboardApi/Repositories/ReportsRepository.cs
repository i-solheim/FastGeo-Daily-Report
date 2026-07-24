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

    public async Task<List<string>> GetProjects()
    {
        var projects = new List<string>();

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
                @"SELECT DISTINCT project
              FROM issues
              ORDER BY project",
                conn);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            projects.Add(reader.GetString(0));

        return projects;
    }

    public async Task<List<ChangeRecord>> GetChanges(
    string project,
    DateOnly day)
    {
        var changes = new List<ChangeRecord>();

        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(
            @"SELECT
            sc.issue_key,
            i.issue_url,
            i.issue_title,
            i.issue_type,
            sc.author,
            sc.changed_at,
            sc.from_status,
            sc.to_status,
            CASE
                WHEN sc.to_status='Done'
                THEN 'completed'
                ELSE 'status_change'
            END AS category

        FROM status_changes sc
        JOIN issues i
            ON sc.issue_key=i.issue_key

        WHERE
            i.project=@project
            AND sc.changed_at::date=@day

        UNION ALL

        SELECT
            i.issue_key,
            i.issue_url,
            i.issue_title,
            i.issue_type,
            i.author,
            i.created_at,
            NULL,
            NULL,
            'new_task'

        FROM issues i

        WHERE
            i.project=@project
            AND i.created_at::date=@day

        ORDER BY author,
                 changed_at;",
            conn);

        cmd.Parameters.AddWithValue("project", project);
        cmd.Parameters.AddWithValue("day", day);

        await using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            changes.Add(new ChangeRecord
            {
                IssueKey = reader.GetString(0),
                IssueUrl = reader.GetString(1),
                IssueTitle = reader.GetString(2),
                IssueType = reader.GetString(3),
                Author = reader.GetString(4),
                ChangedAt = reader.GetDateTime(5),
                FromStatus = reader.IsDBNull(6) ? null : reader.GetString(6),
                ToStatus = reader.IsDBNull(7) ? null : reader.GetString(7),
                Category = reader.GetString(8)});
        }

        return changes;
    }
    public async Task<SummaryResult> GetSummary(
        string project,
        DateOnly day)
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

        await using var cmd =
            new NpgsqlCommand(
            @"SELECT category,
                 COUNT(*)

        FROM
        (
            SELECT
                CASE
                    WHEN sc.to_status='Done'
                    THEN 'completed'
                    ELSE 'status_change'
                END category

            FROM status_changes sc

            JOIN issues i
                ON sc.issue_key=i.issue_key

            WHERE
                i.project=@project
                AND sc.changed_at::date=@day

            UNION ALL

            SELECT 'new_task'

            FROM issues

            WHERE
                project=@project
                AND created_at::date=@day

        ) x

        GROUP BY category;",
            conn);

        cmd.Parameters.AddWithValue("project", project);
        cmd.Parameters.AddWithValue("day", day);

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