namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IAccessTokenService
{

    Task<string> GenerateAccessToken(AdminEntity admin);

    string GetAdminIdFromToken(string token);

    bool ValidateAccessToken(string token);

    Task RevokeAndBlacklistAccessToken(string userId);

    void CleanUpExpiredTokens();
}
