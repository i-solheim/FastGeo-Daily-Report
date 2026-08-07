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

            var user =
                await userRepository.GetByUsername(githubUser.Login);

            if (user == null)
            {
                return Results.Forbid();
            }
            else
            {
                var displayName =
                    string.IsNullOrWhiteSpace(githubUser.Name)
                        ? githubUser.Login
                        : githubUser.Name;

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

        app.MapPost("/api/login", async (
            LoginRequest request,
            UserRepository userRepository,
            PasswordService passwordService,
            JwtService jwtService
        ) =>
        {
            var user = await userRepository.GetByUsername(request.Username);

            if (user == null)
            {
                return Results.Unauthorized();
            }

            var result = passwordService.Verify(user, request.Password);

            var valid =
                result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded;

            if (!valid)
            {
                return Results.Unauthorized();
            }

            var token = jwtService.CreateToken(user);

            return Results.Ok(new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            });
        });

        app.MapGet("/auth/me", (
            ClaimsPrincipal user
        ) =>
        {
            return Results.Ok(new
            {
                Username = user.Identity?.Name,
                DisplayName = user.FindFirst("display_name")?.Value,
                Role = user.FindFirst(ClaimTypes.Role)?.Value
                
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