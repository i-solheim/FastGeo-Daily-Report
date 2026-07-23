namespace DashboardApi.Models;

public class ChangeRecord
{
    public string IssueKey { get; set; } = "";

    public string IssueTitle { get; set; } = "";

    public string IssueType { get; set; } = "";

    public string Author { get; set; } = "";

    public DateTime ChangedAt { get; set; }

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public string Category { get; set; } = "";
}