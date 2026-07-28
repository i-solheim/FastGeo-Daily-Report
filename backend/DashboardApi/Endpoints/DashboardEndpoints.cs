using DashboardApi.Repositories;
using DashboardApi.Services;

namespace DashboardApi.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/projects",
            async (DashboardRepository repo) =>
            {
                return Results.Json(new
                {
                    projects = await repo.GetProjects()
                });
            });

        app.MapGet("/api/projects/{projectKey}/changes",
            async (
                string projectKey,
                string? date,
                DashboardRepository repo) =>
            {
                DateOnly day = DateOnly.Parse(
                    date ??
                    DateTime.Today
                        .AddDays(-1)
                        .ToString("yyyy-MM-dd"));

                var changes =
                    await repo.GetChanges(projectKey, day);

                return Results.Json(new
                {
                    project = projectKey,
                    date = day.ToString("yyyy-MM-dd"),
                    changes
                });
            });

        app.MapGet("/api/projects/{projectKey}/summary",
            async (
                string projectKey,
                string? date,
                DashboardRepository repo) =>
            {
                DateOnly day = DateOnly.Parse(
                    date ??
                    DateTime.Today
                        .AddDays(-1)
                        .ToString("yyyy-MM-dd"));

                return Results.Json(
                    await repo.GetSummary(projectKey, day));
            });

        app.MapGet("/api/projects/{projectKey}/report",
            async (
                string projectKey,
                string? date,
                DashboardRepository repo,
                DailyReportFormatter formatter) =>
            {
                DateOnly day = DateOnly.Parse(
                    date ??
                    DateTime.Today
                        .AddDays(-1)
                        .ToString("yyyy-MM-dd"));

                DateOnly today = DateOnly.Parse(date ??
                    DateTime.Today.ToString("yyyy-MM-dd"));

                DateOnly yesterday = today.AddDays(-1);

                var yesterdayChanges =
                    await repo.GetChanges(projectKey, yesterday);

                var todayChanges =
                    await repo.GetChanges(projectKey, today);

                var report = formatter.Format(
                    yesterdayChanges,
                    todayChanges);

                return Results.Text(report);
            });
    }
}