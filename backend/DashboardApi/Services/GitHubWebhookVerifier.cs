using System.Security.Cryptography;
using System.Text;

public static class GitHubWebhookVerifier
{
    public static bool Verify(
        string body,
        string? signature,
        string secret)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return false;

        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

        using var hmac = new HMACSHA256(secretBytes);

        byte[] hash = hmac.ComputeHash(bodyBytes);

        string expected =
            "sha256=" + Convert.ToHexString(hash).ToLower();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }
}