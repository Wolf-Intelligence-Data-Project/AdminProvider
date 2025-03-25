using Microsoft.AspNetCore.Identity;

namespace AdminProvider.ModeratorsManagement.Interfaces.Utillities;

/// <summary>
/// Custom IPasswordHasher (not Identity) because we are not using dbcontext for saving moderators
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ICustomPasswordHasher<TUser>
{
    string HashPassword(TUser user, string password);
    bool VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword);
}