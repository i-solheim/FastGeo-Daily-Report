using DashboardApi.Models;

namespace DashboardApi.Services;

public class GitHubService
{
  private readonly HttpClient _client;

  public GitHubService(HttpClient client)
  {
    _client = client;
  }

  public async Task<IssueInfo> ResolveIssueFromNodeId(string nodeId)
  {
    const string query = @"
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

    var request = new
    {
      query,
      variables = new { nodeId }
    };

    HttpResponseMessage response =
        await _client.PostAsJsonAsync(
            "https://api.github.com/graphql",
            request);

    response.EnsureSuccessStatusCode();

    GitHubGraphQLResponse? result =
        await response.Content.ReadFromJsonAsync<GitHubGraphQLResponse>();

    if (result?.Data?.Node is null)
      throw new Exception($"Unable to resolve issue '{nodeId}'.");

    var issue = result.Data.Node;

    string owner = issue.Repository.Owner.Login;
    string repo = issue.Repository.Name;

    string issueUrl =
        $"https://github.com/{owner}/{repo}/issues/{issue.Number}";

    return new IssueInfo
    {
      IssueKey =
        $"{repo}#{issue.Number}",

      IssueUrl = issueUrl,

      Project = repo,

      IssueTitle = issue.Title,

      IssueType = "Issue",

      CreatedAt = issue.CreatedAt,

      Author = issue.Author.Login
    };
  }
}