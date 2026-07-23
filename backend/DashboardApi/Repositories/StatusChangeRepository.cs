using DashboardApi.Models;
using Npgsql;
using NpgsqlTypes;

namespace DashboardApi.Repositories;

public class StatusChangeRepository
{
    private readonly string _connectionString;

    public StatusChangeRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")!;
    }

    public async Task SaveStatusChange(
        StatusChange change)
    {
        await using var conn =
            new NpgsqlConnection(_connectionString);

        await conn.OpenAsync();

        await using var cmd =
            new NpgsqlCommand(@"
            INSERT INTO status_changes
                (issue_key,
                 author,
                 changed_at,
                 from_status,
                 to_status)
            VALUES
                (@key,
                 @author,
                 @changed_at,
                 @from_status,
                 @to_status)
            ON CONFLICT
                (issue_key,
                 changed_at,
                 to_status)
            DO NOTHING;",
            conn);

        cmd.Parameters.AddWithValue(
            "key",
            change.IssueKey);

        cmd.Parameters.AddWithValue(
            "author",
            change.Author);

        cmd.Parameters.AddWithValue(
            "changed_at",
            change.ChangedAt);

        cmd.Parameters
            .Add("from_status", NpgsqlDbType.Text)
            .Value =
            (object?)change.FromStatus ?? DBNull.Value;

        cmd.Parameters
            .Add("to_status", NpgsqlDbType.Text)
            .Value =
            (object?)change.ToStatus ?? DBNull.Value;

        await cmd.ExecuteNonQueryAsync();
    }
}