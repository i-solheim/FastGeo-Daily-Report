using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class GitHubOAuthService
{
    private readonly HttpClient _client;
    private readonly string _clientSecret;
    private readonly string _clientId;
    private readonly string _callbackUrl;
    private readonly string _organization;

    public GitHubOAuthService(IConfiguration configuration, HttpClient client)
    {
        _client = client;
        _clientId = configuration["GitHubOAuth:ClientId"]!;
        _callbackUrl = configuration["GitHubOAuth:CallbackUrl"]!;
        _clientSecret = configuration["GitHubOAuth:ClientSecret"]!;
        _organization = configuration["GitHubOAuth:Organization"]!;
    }

    public string BuildAuthorizationUrl()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = _clientId,
            ["redirect_uri"] = _callbackUrl,
            ["scope"] = "read:org"
        };

        return QueryHelpers.AddQueryString(
            "https://github.com/login/oauth/authorize",
            parameters);
    }

    public async Task<GitHubAccessTokenResponse> ExchangeCodeAsync(string code)
    {
        var form = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["code"] = code,
                ["redirect_uri"] = _callbackUrl
            });
        var response = await _client.PostAsync
            ("https://github.com/login/oauth/access_token", form);

        response.EnsureSuccessStatusCode();

        var tokenResponse =
            await response.Content.ReadFromJsonAsync<GitHubAccessTokenResponse>();

        if (tokenResponse == null)
        {
            throw new Exception("GitHub returned an invalid access token response.");
        }

        return tokenResponse;
    }

    public async Task<GitHubUser> GetUserAsync(string accessToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.github.com/user");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<GitHubUser>();

        if (user == null)
        {
            throw new InvalidOperationException(
                "GitHub returned an invalid user response.");
        }

        return user;
    }

    public async Task<bool> IsOrganizationMemberAsync(
        string accessToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.github.com/user/memberships/orgs/{_organization}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var membership =
            await response.Content
                .ReadFromJsonAsync<GitHubOrganizationMembership>();

        return membership?.State == "active";
    }
}