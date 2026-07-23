namespace DashboardApi.Models;

public class StatusChange
{
    public string IssueKey { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }
}