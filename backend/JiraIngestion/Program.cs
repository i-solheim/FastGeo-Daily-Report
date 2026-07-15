using Npgsql;
using System.Text.Json;

// -- CONFIG -- //
string PG_HOST = Environment.GetEnvironmentVariable("PG_HOST");
string PG_PORT = Environment.GetEnvironmentVariable("PG_PORT");
string PG_DATABASE = Environment.GetEnvironmentVariable("PG_DATABASE");
string PG_USERNAME = Environment.GetEnvironmentVariable("PG_USERNAME");
string PG_PASSWORD = Environment.GetEnvironmentVariable("PG_PASSWORD");

string JIRA_URL = Environment.GetEnvironmentVariable("JIRA_URL");
string JIRA_USERNAME = Environment.GetEnvironmentVariable("JIRA_USERNAME");
string JIRA_PASSWORD = Environment.GetEnvironmentVariable("JIRA_PASSWORD");
List<string> PROJECT_KEYS = new List<string>() { "ABP" };
int LOOKBACK_DAYS = 1;

// -- AUTH -- //
string credentials = JIRA_USERNAME + ":" + JIRA_PASSWORD;
byte[] credentialBytes = System.Text.Encoding.UTF8.GetBytes(credentials);
string encodedCredentials = Convert.ToBase64String(credentialBytes);
string header = "Basic " + encodedCredentials;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("Authorization", header);

// -- MAIN -- //
foreach (string projectKey in PROJECT_KEYS)
{
    List<JsonElement> issues = await GetRecentlyUpdatedIssues(projectKey, JIRA_URL, LOOKBACK_DAYS, client);
    List<StatusChange> allChanges = new List<StatusChange>();
    foreach (JsonElement issue in issues)
    {
        allChanges.AddRange(ExtractStatusChanges(issue));
    }

    string connectionString = $"Host={PG_HOST};Port={PG_PORT};Database={PG_DATABASE};Username={PG_USERNAME};Password={PG_PASSWORD}";

    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    foreach (StatusChange change in allChanges)
    {
        await using var cmd = new NpgsqlCommand(
        @"INSERT INTO status_changes
    (project, issue_key, issue_title, issue_type, author, changed_at, from_status, to_status)
    VALUES (@project, @issue_key, @issue_title, @issue_type, @author, @changed_at, @from_status, @to_status)
    ON CONFLICT (issue_key, changed_at, to_status)
    DO UPDATE SET
        project = EXCLUDED.project,
        issue_title = EXCLUDED.issue_title,
        issue_type = EXCLUDED.issue_type,
        author = EXCLUDED.author,
        from_status = EXCLUDED.from_status
    ", conn
    );

        cmd.Parameters.AddWithValue("project", projectKey);
        cmd.Parameters.AddWithValue("issue_key", change.IssueKey);
        cmd.Parameters.AddWithValue("issue_title", change.IssueTitle);
        cmd.Parameters.AddWithValue("issue_type", change.IssueType);
        cmd.Parameters.AddWithValue("author", change.Author);
        cmd.Parameters.AddWithValue("changed_at", change.ChangedAt);
        cmd.Parameters.AddWithValue("from_status", change.FromStatus);
        cmd.Parameters.AddWithValue("to_status", change.ToStatus);

        await cmd.ExecuteNonQueryAsync();
    }


}

// -- METHODS -- //

static async Task<List<JsonElement>> GetRecentlyUpdatedIssues(string projectKey, string jira_url, int lookback_days, HttpClient client)
{
    int startAt = 0;
    int pageSize = 50;
    List<JsonElement> allIssues = new List<JsonElement>();

    while (true)
    {
        string jql = $"project = \"{projectKey}\" AND updated >= -{lookback_days}d";
        string url = $"{jira_url}/rest/api/2/search?jql={Uri.EscapeDataString(jql)}&startAt={startAt}&maxResults={pageSize}&fields=summary,issuetype&expand=changelog";

        HttpResponseMessage searchResponse = await client.GetAsync(url);
        string searchBody = await searchResponse.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(searchBody);
        JsonElement root = doc.RootElement;

        int total = root.GetProperty("total").GetInt32();
        JsonElement issues = root.GetProperty("issues");

        allIssues.AddRange(issues.EnumerateArray());

        if (startAt + pageSize >= total)
        {
            break;
        }
        startAt += pageSize;
    }

    return allIssues;
}

static List<StatusChange> ExtractStatusChanges(JsonElement issue)
{
    List<StatusChange> changes = new List<StatusChange>();
    foreach (JsonElement history in issue.GetProperty("changelog").GetProperty("histories").EnumerateArray())
    {
        foreach (JsonElement item in history.GetProperty("items").EnumerateArray())
        {
            if (item.GetProperty("field").GetString() == "status")
            {
                changes.Add(new StatusChange
                {
                    IssueKey = issue.GetProperty("key").GetString(),
                    IssueTitle = issue.GetProperty("fields").GetProperty("summary").GetString(),
                    IssueType = issue.GetProperty("fields").GetProperty("issuetype").GetProperty("name").GetString(),
                    Author = history.GetProperty("author").GetProperty("displayName").GetString(),
                    ChangedAt = DateTime.Parse(history.GetProperty("created").GetString()),
                    FromStatus = item.GetProperty("fromString").GetString(),
                    ToStatus = item.GetProperty("toString").GetString()
                });
            }
        }
    }
    return changes;
}


// -- CLASS DEFINITIONS -- //
class StatusChange
{
    public string IssueKey { get; set; }
    public string IssueTitle { get; set; }
    public string IssueType { get; set; }
    public string Author { get; set; }
    public DateTime ChangedAt { get; set; }
    public string FromStatus { get; set; }
    public string ToStatus { get; set; }
}