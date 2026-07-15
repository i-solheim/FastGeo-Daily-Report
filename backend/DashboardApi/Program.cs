// Program.cs
//
// Sets up three endpoints:
//   GET /api/projects                      -> list of distinct project keys
//   GET /api/projects/{projectKey}/changes  -> status changes for one project/day, with issue_type and category
//   GET /api/projects/{projectKey}/summary  -> counts of completed/status changes/new tasks for one project/day

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowReactDev");

string ConnectionString = builder.Configuration.GetConnectionString("Postgres");

// ----------------------------
// GET /api/projects
// ----------------------------
app.MapGet("/api/projects", async () =>
{
    var projects = new List<string>();

    await using var conn = new NpgsqlConnection(ConnectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(
        "SELECT DISTINCT project FROM status_changes ORDER BY project", conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        projects.Add(reader.GetString(0));
    }

    return Results.Json(new { projects });
});

// ----------------------------
// GET /api/projects/{projectKey}/changes?date=YYYY-MM-DD
// Now includes issue_type and a computed category per change.
// ----------------------------
app.MapGet("/api/projects/{projectKey}/changes", async (string projectKey, string? date) =>
{
    string day = date ?? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
    var changes = new List<object>();

    await using var conn = new NpgsqlConnection(ConnectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(
        @"SELECT issue_key, issue_title, issue_type, author, changed_at, from_status, to_status
          FROM status_changes
          WHERE project = @project AND changed_at::date = @day
          ORDER BY author, changed_at",
        conn);
    cmd.Parameters.AddWithValue("project", projectKey);
    cmd.Parameters.AddWithValue("day", DateOnly.Parse(day));

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        string fromStatus = reader.GetString(5);
        string toStatus = reader.GetString(6);

        // Rule confirmed so far: Done -> completed, moved out of Backlog -> new_task, else status_change
        // NOTE: still pending final manager confirmation - see the questions you sent.
        string category = toStatus == "Done" ? "completed"
                         : fromStatus == "Backlog" ? "new_task"
                         : "status_change";

        changes.Add(new
        {
            issue_key = reader.GetString(0),
            issue_title = reader.GetString(1),
            issue_type = reader.GetString(2),
            author = reader.GetString(3),
            changed_at = reader.GetDateTime(4).ToString("o"),
            from_status = fromStatus,
            to_status = toStatus,
            category,
        });
    }

    return Results.Json(new { project = projectKey, date = day, changes });
});

// ----------------------------
// GET /api/projects/{projectKey}/summary?date=YYYY-MM-DD
// Returns counts for the summary cards + donut chart.
// ----------------------------
app.MapGet("/api/projects/{projectKey}/summary", async (string projectKey, string? date) =>
{
    string day = date ?? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

    await using var conn = new NpgsqlConnection(ConnectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(
        @"SELECT
            COUNT(*) AS total,
            COUNT(*) FILTER (WHERE to_status = 'Done') AS completed,
            COUNT(*) FILTER (WHERE from_status = 'Backlog') AS new_tasks,
            COUNT(*) FILTER (WHERE to_status != 'Done' AND from_status != 'Backlog') AS status_changes
          FROM status_changes
          WHERE project = @project AND changed_at::date = @day",
        conn);
    cmd.Parameters.AddWithValue("project", projectKey);
    cmd.Parameters.AddWithValue("day", DateOnly.Parse(day));

    await using var reader = await cmd.ExecuteReaderAsync();
    await reader.ReadAsync();

    return Results.Json(new
    {
        project = projectKey,
        date = day,
        total = reader.GetInt32(0),
        completed = reader.GetInt32(1),
        new_tasks = reader.GetInt32(2),
        status_changes = reader.GetInt32(3),
    });
});

app.Run();