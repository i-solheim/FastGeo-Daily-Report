using System.Text.Json;
using DashboardApi.Models;
using DashboardApi.Repositories;

namespace DashboardApi.Services;

public class WebhookService
{
    private readonly GitHubService _github;
    private readonly IssueRepository _issueRepo;
    private readonly StatusChangeRepository _statusRepo;

    public WebhookService(
        GitHubService github,
        IssueRepository issueRepo,
        StatusChangeRepository statusRepo)
    {
        _github = github;
        _issueRepo = issueRepo;
        _statusRepo = statusRepo;
    }

    public async Task ProcessWebhook(
        string eventType,
        JsonElement root)
    {
        if (eventType != "projects_v2_item")
            return;

        string action =
            root.GetProperty("action").GetString()!;

        if (action == "created")
        {
            await HandleCreated(root);
            return;
        }

        if (action == "edited")
        {
            await HandleEdited(root);
        }
    }

    private async Task HandleCreated(JsonElement root)
    {
        JsonElement item =
            root.GetProperty("projects_v2_item");

        string contentNodeId =
            item.GetProperty("content_node_id")
                .GetString()!;

        IssueInfo issue =
            await _github.ResolveIssueFromNodeId(contentNodeId);

        await _issueRepo.UpsertIssue(issue);
    }

    private async Task HandleEdited(JsonElement root)
    {
        if (!root.TryGetProperty("changes", out JsonElement changes) ||
            !changes.TryGetProperty("field_value", out JsonElement fieldValue) ||
            fieldValue.GetProperty("field_name").GetString() != "Status")
        {
            return;
        }

        string sender =
            root.GetProperty("sender")
                .GetProperty("login")
                .GetString()!;

        string contentNodeId =
            root.GetProperty("projects_v2_item")
                .GetProperty("content_node_id")
                .GetString()!;

        IssueInfo issue =
            await _github.ResolveIssueFromNodeId(contentNodeId);

        await _issueRepo.UpsertIssue(issue);

        string? fromStatus =
            fieldValue.GetProperty("from").ValueKind == JsonValueKind.Null
                ? null
                : fieldValue.GetProperty("from")
                    .GetProperty("name")
                    .GetString();

        string? toStatus =
            fieldValue.GetProperty("to").ValueKind == JsonValueKind.Null
                ? null
                : fieldValue.GetProperty("to")
                    .GetProperty("name")
                    .GetString();

        if (string.IsNullOrEmpty(fromStatus))
        {
            return;
        }

        await _statusRepo.SaveStatusChange(
            new StatusChange
            {
                IssueKey = issue.IssueKey,
                Author = sender,
                ChangedAt = DateTime.UtcNow,
                FromStatus = fromStatus,
                ToStatus = toStatus
            });
    }
}