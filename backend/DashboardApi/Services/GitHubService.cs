using System.Net.Http.Json;
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

        return new IssueInfo
        {
            IssueKey =
                $"{issue.Repository.Owner.Login}/{issue.Repository.Name}#{issue.Number}",

            Project = issue.Repository.Name,

            IssueTitle = issue.Title,

            IssueType = "Issue",

            CreatedAt = issue.CreatedAt,

            Author = issue.Author.Login
        };
    }
}