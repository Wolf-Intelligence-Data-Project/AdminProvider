using AdminProvider.ModeratorsManagement.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IAccessTokenService
{

    Task<string> GenerateAccessToken(AdminEntity admin);

    string GetAdminIdFromToken(string token);

    AuthStatus ValidateAccessToken();

    Task RevokeAndBlacklistAccessToken(string userId);

    void CleanUpExpiredTokens();
}
