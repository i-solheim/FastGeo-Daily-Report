using Microsoft.AspNetCore.Identity;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class PasswordService
{
    private readonly PasswordHasher<User> _hasher;
    public PasswordService() {
        _hasher = new PasswordHasher<User>();
    }

    public string Hash(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public PasswordVerificationResult Verify(User user, string password)
    {
        return _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
    }
}