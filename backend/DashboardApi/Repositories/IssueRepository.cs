using DashboardApi.Models;
using Npgsql;

namespace DashboardApi.Repositories;

public class IssueRepository
{
    private readonly string _connectionString;

    public IssueRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")!;
    }

    public async Task UpsertIssue(IssueInfo issue)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO issues
                (issue_key,
                 project,
                 issue_title,
                 issue_type,
                 created_at,
                 author)
            VALUES
                (@key,
                 @project,
                 @title,
                 @type,
                 @created_at,
                 @author)
            ON CONFLICT (issue_key)
            DO NOTHING;",
            conn);

        cmd.Parameters.AddWithValue("key", issue.IssueKey);
        cmd.Parameters.AddWithValue("project", issue.Project);
        cmd.Parameters.AddWithValue("title", issue.IssueTitle);
        cmd.Parameters.AddWithValue("type", issue.IssueType);
        cmd.Parameters.AddWithValue("created_at", issue.CreatedAt);
        cmd.Parameters.AddWithValue("author", issue.Author);

        await cmd.ExecuteNonQueryAsync();
    }
}