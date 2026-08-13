using DashboardApi.Models;
using DashboardApi.Repositories;
using System.Security.Claims;

namespace DashboardApi.Endpoints;

public static class AdminOrLeaderEndpoints
{
    public static void MapAdminOrLeaderEndpoints(this WebApplication app)
    {
        var projectMembers = app
            .MapGroup("/api/projects/{projectKey}/members")
            .RequireAuthorization("AdminOrLeader");

        projectMembers.MapGet("",
            async (
                string projectKey,
                ProjectRepository repo) =>
            {
                var members =
                    await repo.GetMembers(projectKey);

                if (members == null)
                {
                    return Results.NotFound(new
                    {
                        message = "Project not found."
                    });
                }

                return Results.Ok(new
                {
                    members
                });
            });

        projectMembers.MapGet("/available-users",
            async (
                string projectKey,
                ProjectRepository repo) =>
            {
                var users =
                    await repo.GetAvailableUsers(projectKey);

                if (users == null)
                {
                    return Results.NotFound(new
                    {
                        message = "Project not found."
                    });
                }

                return Results.Ok(new
                {
                    users
                });
            });

        projectMembers.MapPost("",
            async (
                ClaimsPrincipal user,
                string projectKey,
                AssignProjectMemberRequest request,
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

                var isAdmin =
                    user.FindFirst("is_admin")?.Value == "true";

                if (!isAdmin && request.Role == "Leader")
                {
                    return Results.Forbid();
                }

                var result = await repo.AddMember(
                    projectKey,
                    request.UserId,
                    request.Role);

                return result switch
                {
                    AddProjectMemberResult.ProjectNotFound =>
                        Results.NotFound(new
                        {
                            message = "Project not found."
                        }),

                    AddProjectMemberResult.UserNotFound =>
                        Results.NotFound(new
                        {
                            message = "User not found."
                        }),

                    AddProjectMemberResult.AlreadyMember =>
                        Results.Conflict(new
                        {
                            message =
                                "User is already a member of this project."
                        }),

                    AddProjectMemberResult.Added =>
                        Results.Ok(new
                        {
                            message = "User added to project."
                        }),

                    _ =>
                        Results.StatusCode(500)
                };
            });

        projectMembers.MapDelete("/{userId}",
            async (
                ClaimsPrincipal user,
                string projectKey,
                int userId,
                ProjectRepository repo) =>
            {
                var isAdmin =
                    user.FindFirst("is_admin")?.Value == "true";

                if (!isAdmin)
                {
                    // Leaders can only remove Members.
                    var membership =
                        await repo.GetMembership(
                            projectKey,
                            userId);

                    if (membership == null)
                    {
                        return Results.NotFound(new
                        {
                            message = "Project membership not found."
                        });
                    }

                    if (membership.Role != "Member")
                    {
                        return Results.Forbid();
                    }
                }

                var result = await repo.RemoveMember(
                    projectKey,
                    userId);

                return result switch
                {
                    RemoveProjectMemberResult.NotFound =>
                        Results.NotFound(new
                        {
                            message =
                                "Project membership not found."
                        }),

                    RemoveProjectMemberResult.LastLeader =>
                        Results.Conflict(new
                        {
                            message =
                                "Cannot remove the last leader from a project."
                        }),

                    RemoveProjectMemberResult.Removed =>
                        Results.Ok(new
                        {
                            message =
                                "User removed from project."
                        }),

                    _ =>
                        Results.StatusCode(500)
                };
            });
    }
}