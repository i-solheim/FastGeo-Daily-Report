using Microsoft.AspNetCore.Identity;
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
            string code,
            GitHubOAuthService githubOAuthService
        ) =>
        {
            var token =
                await githubOAuthService.ExchangeCodeAsync(code);

            return Results.Json(token);
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
    }
}