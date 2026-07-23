// Program.cs
//
// Sets up three endpoints:
//   GET /api/projects                      -> list of distinct project keys
//   GET /api/projects/{projectKey}/changes  -> status changes for one project/day, with issue_type and category
//   GET /api/projects/{projectKey}/summary  -> counts of completed/status changes/new tasks for one project/day

using Npgsql;
using System.Text.Json;

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

// -- CONFIG -- //
string GITHUB_TOKEN = builder.Configuration["GitHub:Token"];
string GITHUB_WEBHOOK_SECRET = builder.Configuration["Github:WebhookSecret"];

// -- AUTH -- //
var client = new HttpClient();
client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GITHUB_TOKEN}");
client.DefaultRequestHeaders.Add("User-Agent", "FastGeo-Daily-Report");


static async Task<IssueInfo> ResolveIssueFromNodeId(string nodeId, HttpClient client)
{
    string query = @"
    query($nodeId: ID!) {
      node(id: $nodeId) {
        ... on Issue {
          number
          title
          createdAt
          author {
            login
          }
          repository {
            name
            owner {
              login
            }
          }
        }
      }
    }";

    var requestBody = new { query, variables = new { nodeId } };
    HttpResponseMessage response = await client.PostAsJsonAsync("https://api.github.com/graphql", requestBody);
    string responseBody = await response.Content.ReadAsStringAsync();

    using var doc = JsonDocument.Parse(responseBody);
    JsonElement node = doc.RootElement.GetProperty("data").GetProperty("node");

    string repoName = node.GetProperty("repository").GetProperty("name").GetString();
    string repoOwner = node.GetProperty("repository").GetProperty("owner").GetProperty("login").GetString();
    int number = node.GetProperty("number").GetInt32();

    return new IssueInfo
    {
        IssueKey = $"{repoOwner}/{repoName}#{number}",
        Project = repoName,
        IssueTitle = node.GetProperty("title").GetString(),
        IssueType = "Issue",
        CreatedAt = node.GetProperty("createdAt").GetDateTime(),
        Author = node.GetProperty("author").GetProperty("login").GetString()
    };
}

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
// ----------------------------
app.MapGet("/api/projects/{projectKey}/changes", async (string projectKey, string? date) =>
{
    string day = date ?? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
    var changes = new List<object>();

    await using var conn = new NpgsqlConnection(ConnectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(
    @"SELECT sc.issue_key, i.issue_title, i.issue_type, sc.author, sc.changed_at,
             sc.from_status, sc.to_status,
             CASE WHEN sc.to_status = 'Done' THEN 'completed' ELSE 'status_change' END AS category
      FROM status_changes sc
      JOIN issues i ON sc.issue_key = i.issue_key
      WHERE i.project = @project AND sc.changed_at::date = @day

      UNION ALL

      SELECT i.issue_key, i.issue_title, i.issue_type, i.author, i.created_at,
             NULL, NULL, 'new_task'
      FROM issues i
      WHERE i.project = @project AND i.created_at::date = @day

      ORDER BY author, changed_at",
    conn);
    cmd.Parameters.AddWithValue("project", projectKey);
    cmd.Parameters.AddWithValue("day", DateOnly.Parse(day));

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        string? fromStatus = reader.IsDBNull(5) ? null : reader.GetString(5);
        string? toStatus = reader.IsDBNull(6) ? null : reader.GetString(6);
        string category = reader.GetString(7);

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
    @"SELECT category, COUNT(*) AS count
      FROM (
          SELECT CASE WHEN sc.to_status = 'Done' THEN 'completed' ELSE 'status_change' END AS category
          FROM status_changes sc
          JOIN issues i ON sc.issue_key = i.issue_key
          WHERE i.project = @project AND sc.changed_at::date = @day

          UNION ALL

          SELECT 'new_task' AS category
          FROM issues
          WHERE project = @project AND created_at::date = @day
      ) combined
      GROUP BY category",
    conn);
    cmd.Parameters.AddWithValue("project", projectKey);
    cmd.Parameters.AddWithValue("day", DateOnly.Parse(day));

    var counts = new Dictionary<string, int>
    {
        ["completed"] = 0,
        ["status_change"] = 0,
        ["new_task"] = 0,
    };

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        counts[reader.GetString(0)] = reader.GetInt32(1);
    }

    return Results.Json(new
    {
        project = projectKey,
        date = day,
        total = counts["completed"] + counts["status_change"] + counts["new_task"],
        completed = counts["completed"],
        new_tasks = counts["new_task"],
        status_changes = counts["status_change"],
    });
});

//
//
//
app.MapPost("/webhooks/github", async (HttpRequest request) =>
{

    using var reader = new StreamReader(request.Body);
    string body = await reader.ReadToEndAsync();

    string? signatureHeader = request.Headers["X-Hub-Signature-256"];
    if (string.IsNullOrEmpty(signatureHeader))
    {
        return Results.Unauthorized();
    }

    byte[] secretBytes = System.Text.Encoding.UTF8.GetBytes(GITHUB_WEBHOOK_SECRET);
    byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);

    using var hmac = new System.Security.Cryptography.HMACSHA256(secretBytes);
    byte[] computedHash = hmac.ComputeHash(bodyBytes);
    string computedSignature = "sha256=" + Convert.ToHexString(computedHash).ToLower();

    if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(computedSignature),
            System.Text.Encoding.UTF8.GetBytes(signatureHeader)))
    {
        return Results.Unauthorized();
    }

    string? eventType = request.Headers["X-GitHub-Event"];

    if (eventType == "ping")
    {
        return Results.Ok();
    }

    Console.WriteLine($"Event: {eventType}");
    Console.WriteLine(body);

    using var doc = JsonDocument.Parse(body);
    JsonElement root = doc.RootElement;

    string action = root.GetProperty("action").GetString()!;

    if (action != "created" && action != "edited")
    {
        return Results.Ok();
    }

    JsonElement item = root.GetProperty("projects_v2_item");
    string contentNodeId = item.GetProperty("content_node_id").GetString()!;

    // --------------------------
    // Item added to project -> new row in `issues`
    // --------------------------
    if (action == "created")
    {
        IssueInfo issue = await ResolveIssueFromNodeId(contentNodeId, client);

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO issues
                (issue_key, project, issue_title, issue_type, created_at, author)
            VALUES
                (@key, @project, @title, @type, @created_at, @author)
            ON CONFLICT (issue_key) DO NOTHING;",
            conn);

        cmd.Parameters.AddWithValue("key", issue.IssueKey);
        cmd.Parameters.AddWithValue("project", issue.Project);
        cmd.Parameters.AddWithValue("title", issue.IssueTitle);
        cmd.Parameters.AddWithValue("type", issue.IssueType);
        cmd.Parameters.AddWithValue("created_at", issue.CreatedAt);
        cmd.Parameters.AddWithValue("author", issue.Author);

        await cmd.ExecuteNonQueryAsync();

        return Results.Ok();
    }

    // --------------------------
    // Status changed -> new row in `status_changes`
    // --------------------------
    if (!root.TryGetProperty("changes", out JsonElement changes) ||
    !changes.TryGetProperty("field_value", out JsonElement fieldValue) ||
    fieldValue.GetProperty("field_name").GetString() != "Status")
    {
        return Results.Ok();
    }

    string sender = root.GetProperty("sender").GetProperty("login").GetString()!;

    IssueInfo issueInfo = await ResolveIssueFromNodeId(contentNodeId, client);

    string? fromStatus = fieldValue.GetProperty("from").ValueKind == JsonValueKind.Null
        ? null
        : fieldValue.GetProperty("from").GetProperty("name").GetString();

    string? toStatus = fieldValue.GetProperty("to").ValueKind == JsonValueKind.Null
        ? null
        : fieldValue.GetProperty("to").GetProperty("name").GetString();

    await using (var conn2 = new NpgsqlConnection(ConnectionString))
    {
        await conn2.OpenAsync();

        // Make sure the issue exists first, since status_changes has a
        // foreign key to issues - protects against receiving an "edited"
        // event for an issue we never saw a "created" event for.
        await using (var upsertIssueCmd = new NpgsqlCommand(@"
            INSERT INTO issues (issue_key, project, issue_title, issue_type, created_at, author)
            VALUES (@key, @project, @title, @type, @created_at, @author)
            ON CONFLICT (issue_key) DO NOTHING;",
            conn2))
        {
            upsertIssueCmd.Parameters.AddWithValue("key", issueInfo.IssueKey);
            upsertIssueCmd.Parameters.AddWithValue("project", issueInfo.Project);
            upsertIssueCmd.Parameters.AddWithValue("title", issueInfo.IssueTitle);
            upsertIssueCmd.Parameters.AddWithValue("type", issueInfo.IssueType);
            upsertIssueCmd.Parameters.AddWithValue("created_at", issueInfo.CreatedAt);
            upsertIssueCmd.Parameters.AddWithValue("author", issueInfo.Author);
            await upsertIssueCmd.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO status_changes
                (issue_key, author, changed_at, from_status, to_status)
            VALUES
                (@key, @author, @changed_at, @from_status, @to_status)
            ON CONFLICT (issue_key, changed_at, to_status) DO NOTHING;",
            conn2);

        cmd.Parameters.AddWithValue("key", issueInfo.IssueKey);
        cmd.Parameters.AddWithValue("author", sender);
        cmd.Parameters.AddWithValue("changed_at", DateTime.UtcNow);
        cmd.Parameters.Add("from_status", NpgsqlTypes.NpgsqlDbType.Text).Value =
    (object?)fromStatus ?? DBNull.Value;

        cmd.Parameters.Add("to_status", NpgsqlTypes.NpgsqlDbType.Text).Value =
            (object?)toStatus ?? DBNull.Value;

        await cmd.ExecuteNonQueryAsync();
    }

    return Results.Ok();
});

// TEMPORARY
app.MapGet("/test/resolve-issue", async (string nodeId) =>
{
    IssueInfo info = await ResolveIssueFromNodeId(nodeId, client);
    return Results.Json(info);
});

app.Run();

class StatusChange
{
    public string IssueKey { get; set; }
    public string Author { get; set; }
    public DateTime ChangedAt { get; set; }
    public string FromStatus { get; set; }
    public string ToStatus { get; set; }
}

class IssueInfo
{
    public string IssueKey { get; set; }
    public string Project { get; set; }
    public string IssueTitle { get; set; }
    public string IssueType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
}