using Microsoft.AspNetCore.WebUtilities;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class GitHubOAuthService
{
    private readonly HttpClient _client;
    private readonly string _clientSecret;
    private readonly string _clientId;
    private readonly string _callbackUrl;

    public GitHubOAuthService(IConfiguration configuration, HttpClient client)
    {
        _client = client;
        _clientId = configuration["GitHubOAuth:ClientId"]!;
        _callbackUrl = configuration["GitHubOAuth:CallbackUrl"]!;
        _clientSecret = configuration["GitHubOAuth:ClientSecret"]!;
    }

    public string BuildAuthorizationUrl()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = _clientId,
            ["redirect_uri"] = _callbackUrl
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
}