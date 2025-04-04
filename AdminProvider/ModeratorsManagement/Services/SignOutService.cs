using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Models.Requests;

namespace AdminProvider.ModeratorsManagement.Services;

public class SignOutService : ISignOutService
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly ILogger<SignOutService> _logger;

    public SignOutService(IAccessTokenService accessTokenService, ILogger<SignOutService> logger)
    {
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Removes the specified token to complete the sign-out process.
    /// </summary>
    /// <param name="token">The token to be removed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a boolean indicating success or failure of the operation.
    /// </returns>
    public async Task<bool> SignOutAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Sign-out failed: Token is null or empty.");
            return false;
        }

        try
        {
            // Check if the token is valid and retrieve the authentication and verification statuses
            var isAuthenticated = _accessTokenService.ValidateAccessToken(); // Retrieve authentication and verification status

            // Log if token is invalid or expired, but allow sign-out regardless
            if (!isAuthenticated.IsAuthenticated)
            {
                _logger.LogInformation("Token is invalid or expired, proceeding with sign-out.");
            }

            // You can still revoke the token even if it's invalid or expired
            var adminId = _accessTokenService.GetAdminIdFromToken(token); // Get adminId from token if possible


            // Revoke the token
            await _accessTokenService.RevokeAndBlacklistAccessToken(adminId); // Revoke token from service

            _logger.LogInformation("Token successfully removed during sign-out.");
            return true;  // Return true as token revocation was successful
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing the token during sign-out.");
            return false;
        }
    }
}