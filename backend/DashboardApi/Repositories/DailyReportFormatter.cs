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
            var yesterdayChanges =
                yesterdayAuthors.TryGetValue(author, out var y)
                    ? y
                    : new List<ChangeRecord>();

            var todayChanges =
                todayAuthors.TryGetValue(author, out var t)
                    ? t
                    : new List<ChangeRecord>();

            // Only include the author if they have
            // at least one reportable change.
            if (!HasReportableChanges(yesterdayChanges) &&
                !HasReportableChanges(todayChanges))
            {
                continue;
            }

            sb.AppendLine($"## {author}");
            sb.AppendLine();

            WriteDay(
                sb,
                "Yesterday",
                yesterdayChanges);

            WriteDay(
                sb,
                "Today",
                todayChanges);

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static void WriteDay(
        StringBuilder sb,
        string heading,
        List<ChangeRecord> changes)
    {
        var done = changes
            .Where(c =>
                c.ToStatus == "In review" ||
                c.ToStatus == "Done")
            .ToList();

        var inProgress = changes
            .Where(c =>
                c.ToStatus == "In progress")
            .ToList();

        if (!done.Any() && !inProgress.Any())
        {
            return;
        }

        sb.AppendLine($"### {heading}");
        sb.AppendLine();

        if (done.Any())
        {
            sb.AppendLine("**Done**");

            foreach (var issue in done)
            {
                sb.AppendLine(
                    $"- **{issue.IssueKey}**: {issue.IssueTitle}");
            }

            sb.AppendLine();
        }

        if (inProgress.Any())
        {
            sb.AppendLine("**In progress**");

            foreach (var issue in inProgress)
            {
                sb.AppendLine(
                    $"- **{issue.IssueKey}**: {issue.IssueTitle}");
            }

            sb.AppendLine();
        }
    }

    private static bool HasReportableChanges(
        List<ChangeRecord> changes)
    {
        return changes.Any(c =>
            c.ToStatus == "In review" ||
            c.ToStatus == "Done" ||
            c.ToStatus == "In progress");
    }
}