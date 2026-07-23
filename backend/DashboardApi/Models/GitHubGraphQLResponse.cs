using System.Text.Json.Serialization;

namespace DashboardApi.Models;

public class GitHubGraphQLResponse
{
    [JsonPropertyName("data")]
    public GraphQLData Data { get; set; } = new();
}

public class GraphQLData
{
    [JsonPropertyName("node")]
    public GraphQLIssue? Node { get; set; }
}

public class GraphQLIssue
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("author")]
    public GraphQLAuthor Author { get; set; } = new();

    [JsonPropertyName("repository")]
    public GraphQLRepository Repository { get; set; } = new();
}

public class GraphQLAuthor
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}

public class GraphQLRepository
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public GraphQLOwner Owner { get; set; } = new();
}

public class GraphQLOwner
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}