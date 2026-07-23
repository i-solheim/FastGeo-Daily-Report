namespace DashboardApi.Models;

public class IssueInfo
{
    public string IssueKey { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string IssueTitle { get; set; } = string.Empty;

    public string IssueType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Author { get; set; } = string.Empty;
}