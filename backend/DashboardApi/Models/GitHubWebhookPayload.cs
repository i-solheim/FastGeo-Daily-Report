using System.Text.Json.Serialization;

namespace DashboardApi.Models;

public class GitHubWebhook
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("projects_v2_item")]
    public ProjectItem ProjectItem { get; set; } = new();

    [JsonPropertyName("changes")]
    public Changes? Changes { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; } = new();
}

public class ProjectItem
{
    [JsonPropertyName("content_node_id")]
    public string ContentNodeId { get; set; } = string.Empty;
}

public class Sender
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}

public class Changes
{
    [JsonPropertyName("field_value")]
    public FieldValue? FieldValue { get; set; }
}

public class FieldValue
{
    [JsonPropertyName("field_name")]
    public string FieldName { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public StatusOption? From { get; set; }

    [JsonPropertyName("to")]
    public StatusOption? To { get; set; }
}

public class StatusOption
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}