using Microsoft.AspNetCore.Identity;

namespace AdminProvider.ModeratorsManagement.Interfaces.Utillities;

/// <summary>
/// Custom IPasswordHasher (not Identity) because we are not using dbcontext for saving moderators
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ICustomPasswordHasher<TUser>
{
    string HashPassword(string password);
    bool VerifyHashedPassword(AdminEntity user, string hashedPassword, string providedPassword);
}