using Microsoft.AspNetCore.WebUtilities;

namespace DashboardApi.Services;

public class GitHubOAuthService
{
    private readonly string _clientId;
    private readonly string _callbackUrl;

    public GitHubOAuthService(IConfiguration configuration)
    {
        _clientId = configuration["GitHubOAuth:ClientId"]!;
        _callbackUrl = configuration["GitHubOAuth:CallbackUrl"]!;
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
}