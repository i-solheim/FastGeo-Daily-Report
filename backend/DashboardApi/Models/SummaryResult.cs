namespace DashboardApi.Models;

public class SummaryResult
{
    public string Project { get; set; } = string.Empty;

    public string Date { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Completed { get; set; }

    public int NewTasks { get; set; }

    public int StatusChanges { get; set; }
}