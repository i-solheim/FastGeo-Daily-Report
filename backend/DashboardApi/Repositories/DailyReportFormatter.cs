using System.Text;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class DailyReportFormatter
{
    public string Format(
        List<ChangeRecord> yesterday,
        List<ChangeRecord> today)
    {
        var sb = new StringBuilder();

        var yesterdayAuthors = yesterday
            .GroupBy(c => c.Author)
            .ToDictionary(g => g.Key, g => g.ToList());

        var todayAuthors = today
            .GroupBy(c => c.Author)
            .ToDictionary(g => g.Key, g => g.ToList());

        var authors = yesterdayAuthors.Keys
            .Union(todayAuthors.Keys)
            .OrderBy(a => a);

        foreach (var author in authors)
        {
            sb.AppendLine(author);

            var yesterdayChanges =
                yesterdayAuthors.TryGetValue(author, out var y)
                    ? y
                    : new List<ChangeRecord>();

            var todayChanges =
                todayAuthors.TryGetValue(author, out var t)
                    ? t
                    : new List<ChangeRecord>();

            WriteDay(sb, "Yesterday\n", yesterdayChanges);
            WriteDay(sb, "Today\n", todayChanges);

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void WriteDay(
        StringBuilder sb,
        string heading,
        List<ChangeRecord> changes)
    {
        sb.AppendLine(heading);

        var done = changes
            .Where(c => c.ToStatus == "In review" || c.ToStatus == "Done")
            .ToList();

        if (done.Any())
        {
            sb.AppendLine("  • Done");

            foreach (var issue in done)
            {
                sb.AppendLine(
                    $"    - {issue.IssueKey}: {issue.IssueTitle}");
            }

        }

        var inProgress = changes
            .Where(c => c.ToStatus == "In progress")
            .ToList();

        if (inProgress.Any())
        {
            sb.AppendLine("  • In progress");

            foreach (var issue in inProgress)
            {
                sb.AppendLine(
                    $"    - {issue.IssueKey}: {issue.IssueTitle}");
            }
        }

        sb.AppendLine();
    }
}