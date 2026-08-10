using System.Security.Claims;
using DashboardApi.Repositories;
using DashboardApi.Services;

namespace DashboardApi.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var dashboard = app
                .MapGroup("/api")
                .RequireAuthorization();

        dashboard.MapGet("/projects",
            async (DashboardRepository repo) =>
            {
                return Results.Json(new
                {
                    projects = await repo.GetProjects()
                });
            });

        dashboard.MapGet("/projects/{projectKey}/changes",
            async (
                string projectKey,
                string? date,
                ClaimsPrincipal user,
                DashboardRepository repo,
                ProjectRepository projectRepository) =>
            {
                DateOnly day = DateOnly.Parse(
                    date ??
                    DateTime.Today
                        .AddDays(-1)
                        .ToString("yyyy-MM-dd"));

                var username = user.Identity?.Name;

                var userIdClaim =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (username == null ||
                    !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var membership =
                    await projectRepository.GetMembership(
                        projectKey,
                        userId);

                if (membership == null)
                {
                    return Results.Forbid();
                }

                var role = membership.Role;

                var changes = await repo.GetChanges(
                    projectKey,
                    day,
                    username,
                    role);

                return Results.Json(new
                {
                    project = projectKey,
                    date = day.ToString("yyyy-MM-dd"),
                    changes
                });
            });

        dashboard.MapGet("/projects/{projectKey}/summary",
            async (
                string projectKey,
                string? date,
                ClaimsPrincipal user,
                DashboardRepository repo,
                ProjectRepository projectRepository) =>
            {
                DateOnly day = DateOnly.Parse(
                    date ??
                    DateTime.Today
                        .AddDays(-1)
                        .ToString("yyyy-MM-dd"));

                var username = user.Identity?.Name;

                var userIdClaim =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (username == null ||
                    !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var membership =
                    await projectRepository.GetMembership(
                        projectKey,
                        userId);

                if (membership == null)
                {
                    return Results.Forbid();
                }

                var role = membership.Role;

                return Results.Json(
                    await repo.GetSummary(
                        projectKey,
                        day,
                        username,
                        role));
            });

        dashboard.MapGet("/projects/{projectKey}/report",
            async (
                string projectKey,
                string? date,
                ClaimsPrincipal user,
                DashboardRepository repo,
                ProjectRepository projectRepository,
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

                var username = user.Identity?.Name;

                var userIdClaim =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (username == null ||
                    !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var membership =
                    await projectRepository.GetMembership(
                        projectKey,
                        userId);

                if (membership == null)
                {
                    return Results.Forbid();
                }

                var role = membership.Role;

                var yesterdayChanges =
                    await repo.GetChanges(projectKey, yesterday, username, role);

                var todayChanges =
                    await repo.GetChanges(projectKey, today, username, role);

                var report = formatter.Format(
                    yesterdayChanges,
                    todayChanges);

                return Results.Text(report);
            });
    }
}