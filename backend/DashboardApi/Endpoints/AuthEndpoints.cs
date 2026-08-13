using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using DashboardApi.Repositories;
using DashboardApi.Services;
using DashboardApi.Models;

namespace DashboardApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/github/login", (
            GitHubOAuthService githubOAuthService
        ) =>
        {
            return Results.Redirect(githubOAuthService.BuildAuthorizationUrl());
        });

        app.MapGet("/auth/github/callback", async (
            HttpContext httpContext,
            string code,
            GitHubOAuthService githubOAuthService,
            UserRepository userRepository,
            JwtService jwtService
        ) =>
        {
            var token =
                await githubOAuthService.ExchangeCodeAsync(code);

            var githubUser =
                await githubOAuthService.GetUserAsync(token.AccessToken);

            var isOrganizationMember =
                await githubOAuthService.IsOrganizationMemberAsync(
                    token.AccessToken);

            if (!isOrganizationMember)
            {
                return Results.Forbid();
            }

            var displayName =
                    string.IsNullOrWhiteSpace(githubUser.Name)
                        ? githubUser.Login
                        : githubUser.Name;

            var user =
                await userRepository.GetByUsername(githubUser.Login);

            if (user == null)
            {
                user = await userRepository.CreateUser(
                    githubUser.Login,
                    githubUser.Name ?? githubUser.Login);
            }
            else
            {
                await userRepository.UpdateDisplayName(
                    user.Id,
                    displayName);

                user.DisplayName = displayName;
            }

            var jwt = jwtService.CreateToken(user);

            httpContext.Response.Cookies.Append(
                "auth",
                jwt,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // localhost only
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

            return Results.Redirect("http://localhost:5173/");
        });

        app.MapGet("/auth/me", (
            ClaimsPrincipal user
        ) =>
        {
            return Results.Ok(new
            {
                Username = user.Identity?.Name,
                DisplayName = user.FindFirst("display_name")?.Value,
                IsAdmin = user.FindFirst("is_admin")?.Value == "true"
            });
        })
        .RequireAuthorization();

        app.MapPost("/auth/logout", (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete("auth", new CookieOptions
            {
                Path = "/"
            });

            return Results.Ok();
        });
    }
}