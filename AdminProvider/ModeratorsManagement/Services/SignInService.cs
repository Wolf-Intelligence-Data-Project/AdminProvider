using Microsoft.AspNetCore.Identity;
using AdminProvider.ModeratorsManagement.Models.Responses;
using AuthenticationProvider.Models.Requests;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;

namespace AdminProvider.ModeratorsManagement.Services;

/// <summary>
/// Service responsible for handling sign-in logic for user authentication.
/// </summary>
public class SignInService : ISignInService
{
    private readonly IConfiguration _configuration;
    private readonly IAccessTokenService _accessTokenService;
    private readonly ICustomPasswordHasher<AdminEntity> _customPasswordHasher;
    private readonly ILogger<SignInService> _logger;

    public SignInService(
        IConfiguration configuration,
        IAccessTokenService accessTokenService,
        ICustomPasswordHasher<AdminEntity> customPasswordHasher,
        ILogger<SignInService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
        _customPasswordHasher = customPasswordHasher ?? throw new ArgumentNullException(nameof(customPasswordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts to sign in a user using email and password.
    /// Validates the provided credentials and generates an access token upon successful authentication.
    /// </summary>
    /// <param name="signInDto">The sign-in request containing email and password.</param>
    /// <returns>A response indicating success or failure, including an access token if successful.</returns>
    public async Task<SignInResponse> SignInAsync(SignInRequest signInRequest)
    {
        if (signInRequest == null)
        {
            _logger.LogWarning("SignInDto is null.");
            return new SignInResponse
            {
                Success = false,
                ErrorMessage = "Ogiltig inloggningsförfrågan."
            };
        }

        if (string.IsNullOrWhiteSpace(signInRequest.Email) || string.IsNullOrWhiteSpace(signInRequest.Password))
        {
            _logger.LogWarning("Sign-in failed: Email or password is empty.");
            return new SignInResponse
            {
                Success = false,
                ErrorMessage = "E-post och lösenord krävs."
            };
        }

        try
        {
            // Retrieve the admins from the configuration (appsettings.json)
            var admins = _configuration.GetSection("Admins").Get<List<AdminEntity>>() ?? new List<AdminEntity>();
            var adminEntity = admins.FirstOrDefault(a => a.Email == signInRequest.Email);

            if (adminEntity == null)
            {
                _logger.LogWarning("Sign-in failed: User not found for provided email.");
                return new SignInResponse
                {
                    Success = false,
                    ErrorMessage = "Användaren finns inte."
                };
            }

            // Validate password
            if (!ValidatePassword(adminEntity, signInRequest.Password))
            {
                _logger.LogWarning("Sign-in failed: Invalid credentials for the provided email.");
                return new SignInResponse
                {
                    Success = false,
                    ErrorMessage = "Felaktiga inloggningsuppgifter."
                };
            }

            string token = await _accessTokenService.GenerateAccessToken(adminEntity);
            if (token == null)
            {
                return new SignInResponse
                {
                    Success = false,
                    Message = "Inloggning lyckades.",
                };
            }

            _logger.LogInformation("User signed in successfully.");
            return new SignInResponse
            {
                Success = true,
                Message = "Inloggning lyckades.",
                Jwt = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during sign-in.");
            return new SignInResponse
            {
                Success = false,
                ErrorMessage = "Ett fel inträffade vid inloggning. Försök igen senare."
            };
        }
    }

    private bool ValidatePassword(AdminEntity admin, string providedPassword)
    {
        // Use the custom password hasher to validate the provided password against the stored hash
        bool isValid = _customPasswordHasher.VerifyHashedPassword(admin, admin.PasswordHash, providedPassword);
        return isValid;
    }

}
