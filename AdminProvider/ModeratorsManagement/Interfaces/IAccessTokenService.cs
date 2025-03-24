using AdminProvider.ModeratorsManagement.Data.Entities;
using AdminProvider.UsersManagement.Data.Entities;

namespace AdminProvider.ModeratorsManagement.Interfaces;

public interface IAccessTokenService
{

    Task<string> GenerateAccessToken(AdminEntity admin);

    string GetUserIdFromToken(string token);

    bool ValidateAccessToken(string token);

    Task RevokeAndBlacklistAccessToken(string userId);

    void CleanUpExpiredTokens();
}
