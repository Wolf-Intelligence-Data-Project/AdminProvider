using Microsoft.AspNetCore.Identity;
using AdminProvider.ModeratorsManagement.Interfaces;
using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.ModeratorsManagement.Models.Responses;
using AdminProvider.ModeratorsManagement.Data.Entities;
using AuthenticationProvider.Models.Requests;

namespace AdminProvider.ModeratorsManagement.Services;

/// <summary>
/// Service responsible for handling sign-in logic for user authentication.
/// </summary>
public class SignInService : ISignInService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IPasswordHasher<AdminEntity> _passwordHasher;
    private readonly ILogger<SignInService> _logger;

    public SignInService(
        IUserRepository adminRepository,
        IAccessTokenService accessTokenService,
        IPasswordHasher<UserEntity> passwordHasher,
        ILogger<SignInService> logger)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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
            // Retrieve the user by email
            var adminEntity = await _adminRepository.GetByEmailAsync(signInRequest.Email);
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

            // Generate access token and store in HTTP-only cookie
            var token = _accessTokenService.GenerateAccessToken(adminEntity);
            if (token == null)
            {
                return new SignInResponse
                {
                    Success = false,
                    Message = "Inloggning lyckades.",
                    Admin = adminEntity, // You may exclude the user if you only rely on the cookie

                };
            }

            _logger.LogInformation("User signed in successfully.");
            return new SignInResponse
            {
                Success = true,
                Message = "Inloggning lyckades.",
                Admin = adminEntity, // You may exclude the user if you only rely on the cookie

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
        var passwordResult = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, providedPassword);
        return passwordResult == PasswordVerificationResult.Success;
    }
}
