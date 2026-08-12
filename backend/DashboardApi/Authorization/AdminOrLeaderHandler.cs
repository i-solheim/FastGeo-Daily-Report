using Microsoft.AspNetCore.Authorization;
using DashboardApi.Repositories;

namespace DashboardApi.Authorization;

public class AdminOrLeaderHandler
    : AuthorizationHandler<AdminOrLeaderRequirement>
{
    private readonly ProjectRepository _projectRepository;

    public AdminOrLeaderHandler(
        ProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOrLeaderRequirement requirement)
    {
        // Admins are always authorized.
        var isAdminClaim =
            context.User.FindFirst("is_admin")?.Value;

        if (isAdminClaim == "true")
        {
            context.Succeed(requirement);
            return;
        }

        // Get projectKey from the route.
        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        var projectKey =
            httpContext.Request.RouteValues["projectKey"]
                ?.ToString();

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            return;
        }

        // Get the authenticated user's ID.
        var userIdClaim =
            context.User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        // Check whether this user is a Leader
        // for this particular project.
        var membership =
            await _projectRepository.GetMembership(
                projectKey,
                userId);

        if (membership?.Role == "Leader")
        {
            context.Succeed(requirement);
        }
    }
}