using DashboardApi.Repositories;
using DashboardApi.Models;

namespace DashboardApi.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var admin = app
            .MapGroup("/api/admin")
            .RequireAuthorization("AdminOnly");

        admin.MapGet("/users",
            async (UserRepository repo) =>
            {
                return Results.Json(new
                {
                    users = await repo.GetUsers()
                });
            });

        admin.MapPatch("/projects/{projectKey}/members/{userId}",
            async (
                string projectKey,
                int userId,
                UpdateProjectMemberRoleRequest request,
                ProjectRepository repo) =>
            {
                if (request.Role != "Leader" &&
                    request.Role != "Member")
                {
                    return Results.BadRequest(new
                    {
                        message = "Role must be Leader or Member."
                    });
                }

                var updated = await repo.UpdateRole(
                    projectKey,
                    userId,
                    request.Role);

                if (!updated)
                {
                    return Results.NotFound(new
                    {
                        message = "Project membership not found."
                    });
                }

                return Results.Ok(new
                {
                    message = "Member role updated."
                });
            });
    }
}