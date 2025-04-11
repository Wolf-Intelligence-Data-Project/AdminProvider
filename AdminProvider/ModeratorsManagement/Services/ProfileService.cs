using AdminProvider.ModeratorsManagement.Data.Entities;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace AdminProvider.ModeratorsManagement.Services;

public class ProfileService : IProfileService
{
    private readonly IAdminService _adminService;
    private readonly ICustomPasswordHasher<AdminEntity> _customPasswordHasher;
    private readonly ILogger<ProfileService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ProfileService(IAdminService adminService, ICustomPasswordHasher<AdminEntity> customPasswordHasher, ILogger<ProfileService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _adminService = adminService;
        _customPasswordHasher = customPasswordHasher;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AdminDto> GetModeratorAsync()
    {
        const string cookieName = "AccessToken";

        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogError("HttpContext is null.");
            throw new UnauthorizedAccessException("HttpContext is missing.");
        }

        var cookies = _httpContextAccessor.HttpContext.Request.Cookies;

        if (!cookies.TryGetValue(cookieName, out var token) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Access token not found in cookies.");
            throw new UnauthorizedAccessException("Access token is missing.");
        }

        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JWT token.");
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        var adminIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "adminId");
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out var adminId))
        {
            _logger.LogWarning("Invalid or missing AdminId claim.");
            throw new UnauthorizedAccessException("Invalid token claims.");
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var json = await File.ReadAllTextAsync(filePath);
        var jsonObj = JObject.Parse(json);

        var adminsArray = (JArray?)jsonObj["Admins"];
        if (adminsArray == null)
        {
            _logger.LogError("Admins array is missing in appsettings.json.");
            throw new InvalidOperationException("No moderators found.");
        }

        var foundAdmin = adminsArray.FirstOrDefault(a =>
            Guid.TryParse(a["AdminId"]?.ToString(), out var id) && id == adminId);

        if (foundAdmin == null)
        {
            _logger.LogWarning("No moderator found with AdminId: {AdminId}", adminId);
            throw new KeyNotFoundException("Moderator not found.");
        }

        return new AdminDto
        {
            AdminId = adminId,
            Email = foundAdmin["Email"]?.ToString(),
            Role = foundAdmin["Role"]?.ToString(),
            FullName = foundAdmin["FullName"]?.ToString(),
            PhoneNumber = foundAdmin["PhoneNumber"]?.ToString(),
            IdentificationNumber = foundAdmin["IdentificationNumber"]?.ToString()
        };
    }
    private async Task<JObject> GetModeratorCompleteAsync()
    {
        const string cookieName = "AccessToken";

        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogError("HttpContext is null.");
            throw new UnauthorizedAccessException("HttpContext is missing.");
        }

        var cookies = _httpContextAccessor.HttpContext.Request.Cookies;

        // Check for the access token in the cookies
        if (!cookies.TryGetValue(cookieName, out var token) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Access token not found in cookies.");
            throw new UnauthorizedAccessException("Access token is missing.");
        }

        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token); // Parse the JWT token
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JWT token.");
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        // Extract adminId from the JWT token claims
        var adminIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "adminId");
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out var adminId))
        {
            _logger.LogWarning("Invalid or missing AdminId claim.");
            throw new UnauthorizedAccessException("Invalid token claims.");
        }

        // Read the admin data from appsettings.json
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var json = await File.ReadAllTextAsync(filePath);
        var jsonObj = JObject.Parse(json);

        var adminsArray = (JArray?)jsonObj["Admins"];
        if (adminsArray == null)
        {
            _logger.LogError("Admins array is missing in appsettings.json.");
            throw new InvalidOperationException("No moderators found.");
        }

        // Find the admin based on the adminId from the JWT token
        var foundAdmin = adminsArray.FirstOrDefault(a =>
            Guid.TryParse(a["AdminId"]?.ToString(), out var id) && id == adminId);

        if (foundAdmin == null)
        {
            _logger.LogWarning("No moderator found with AdminId: {AdminId}", adminId);
            throw new KeyNotFoundException("Moderator not found.");
        }
        _logger.LogWarning(foundAdmin.ToString());
        // Cast foundAdmin to JObject before returning
        return (JObject)foundAdmin; // Explicitly cast to JObject
    }


    public async Task PasswordChangeAsync(PasswordChangeRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Password change request is null.");
            throw new ArgumentNullException(nameof(request), "Password change request cannot be null");
        }

        try
        {
            _logger.LogInformation("Processing password change request for current moderator...");

            var moderator = await GetModeratorCompleteAsync();
            if (moderator == null)
            {
                _logger.LogWarning("Moderator could not be found or token is invalid.");
                throw new InvalidOperationException("Moderator not found or not authorized.");
            }

            var moderatorId = moderator["AdminId"]?.ToString();
            if (!Guid.TryParse(moderatorId, out var adminId))
            {
                _logger.LogWarning("Moderator AdminId is invalid or missing.");
                throw new InvalidOperationException("Invalid moderator ID.");
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string json = await File.ReadAllTextAsync(filePath);
            var configRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (!configRoot.TryGetValue("Admins", out var adminsToken) || adminsToken is not JArray)
            {
                _logger.LogError("The 'Admins' section in appsettings.json is missing or not an array.");
                throw new InvalidOperationException("Admins section is invalid or missing.");
            }

            var admins = JsonConvert.DeserializeObject<List<AdminEntity>>(adminsToken.ToString());
            var adminEntity = admins.FirstOrDefault(a => a.AdminId == adminId);

            if (adminEntity == null)
            {
                _logger.LogWarning("No admin found with ID {AdminId}.", adminId);
                throw new InvalidOperationException("Ingen administratör hittades.");
            }

            // 🔐 1️⃣ Verify CURRENT password
            var isCurrentPasswordValid = _customPasswordHasher.VerifyHashedPassword(
                adminEntity,
                adminEntity.PasswordHash,
                request.CurrentPassword // <-- THIS is the correct password to verify
            );

            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Password change failed: Current password is incorrect.");
                throw new InvalidOperationException("Felaktiga uppgifter."); // Incorrect credentials
            }

            // 🔁 2️⃣ Check if new passwords match
            if (request.NewPassword != request.ConfirmPassword)
            {
                _logger.LogWarning("Password change failed: New password and confirmation do not match.");
                throw new InvalidOperationException("Lösenorden stämmer inte överens.");
            }

            // 🔐 3️⃣ Update password
            var hashedPassword = _customPasswordHasher.HashPassword(request.NewPassword);
            adminEntity.PasswordHash = hashedPassword;
            adminEntity.PasswordChosen = true;

            configRoot["Admins"] = admins;
            string updatedJson = JsonConvert.SerializeObject(configRoot, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, updatedJson);

            _logger.LogInformation($"Password successfully updated for Admin ID: {adminId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the password.");
            throw;
        }
    }

    public async Task EmailChangeAsync(EmailChangeRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Email change request is null.");
            throw new ArgumentNullException(nameof(request), "Email change request cannot be null");
        }

        try
        {
            _logger.LogInformation("Processing email change request for current moderator...");

            var moderator = await GetModeratorCompleteAsync();
            if (moderator == null)
            {
                _logger.LogWarning("Moderator could not be found or token is invalid.");
                throw new InvalidOperationException("Moderator not found or not authorized.");
            }

            var moderatorId = moderator["AdminId"]?.ToString();
            if (!Guid.TryParse(moderatorId, out var adminId))
            {
                _logger.LogWarning("Moderator AdminId is invalid or missing.");
                throw new InvalidOperationException("Ogiltigt moderator-ID.");
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string json = await File.ReadAllTextAsync(filePath);
            var configRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (!configRoot.TryGetValue("Admins", out var adminsToken) || adminsToken is not JArray)
            {
                _logger.LogError("The 'Admins' section in appsettings.json is missing or not an array.");
                throw new InvalidOperationException("Admins-sektionen saknas eller är ogiltig.");
            }

            var admins = JsonConvert.DeserializeObject<List<AdminEntity>>(adminsToken.ToString());
            var adminEntity = admins.FirstOrDefault(a => a.AdminId == adminId);

            if (adminEntity == null)
            {
                _logger.LogWarning("No admin found with ID {AdminId}.", adminId);
                throw new InvalidOperationException("Ingen administratör hittades.");
            }

            // 🔐 Verify current password
            var isCurrentPasswordValid = _customPasswordHasher.VerifyHashedPassword(
                adminEntity,
                adminEntity.PasswordHash,
                request.CurrentPassword
            );

            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Email change failed: Current password is incorrect.");
                throw new InvalidOperationException("Felaktiga uppgifter.");
            }

            // 📧 Update email
            adminEntity.Email = request.Email;

            configRoot["Admins"] = admins;
            string updatedJson = JsonConvert.SerializeObject(configRoot, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, updatedJson);

            _logger.LogInformation($"Email successfully updated for Admin ID: {adminId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the email.");
            throw;
        }
    }
    public async Task PhoneNumberChangeAsync(PhoneNumberChangeRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Phone number change request is null.");
            throw new ArgumentNullException(nameof(request), "Phone number change request cannot be null");
        }

        try
        {
            _logger.LogInformation("Processing phone number change request for current moderator...");

            var moderator = await GetModeratorCompleteAsync();
            if (moderator == null)
            {
                _logger.LogWarning("Moderator could not be found or token is invalid.");
                throw new InvalidOperationException("Moderator not found or not authorized.");
            }

            var moderatorId = moderator["AdminId"]?.ToString();
            if (!Guid.TryParse(moderatorId, out var adminId))
            {
                _logger.LogWarning("Moderator AdminId is invalid or missing.");
                throw new InvalidOperationException("Ogiltigt moderator-ID.");
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string json = await File.ReadAllTextAsync(filePath);
            var configRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (!configRoot.TryGetValue("Admins", out var adminsToken) || adminsToken is not JArray)
            {
                _logger.LogError("The 'Admins' section in appsettings.json is missing or not an array.");
                throw new InvalidOperationException("Admins-sektionen saknas eller är ogiltig.");
            }

            var admins = JsonConvert.DeserializeObject<List<AdminEntity>>(adminsToken.ToString());
            var adminEntity = admins.FirstOrDefault(a => a.AdminId == adminId);

            if (adminEntity == null)
            {
                _logger.LogWarning("No admin found with ID {AdminId}.", adminId);
                throw new InvalidOperationException("Ingen administratör hittades.");
            }

            // 🔐 Verify current password
            var isCurrentPasswordValid = _customPasswordHasher.VerifyHashedPassword(
                adminEntity,
                adminEntity.PasswordHash,
                request.CurrentPassword
            );

            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Phone number change failed: Current password is incorrect.");
                throw new InvalidOperationException("Felaktiga uppgifter.");
            }

            // ☎️ Update phone number
            adminEntity.PhoneNumber = request.PhoneNumber;

            configRoot["Admins"] = admins;
            string updatedJson = JsonConvert.SerializeObject(configRoot, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, updatedJson);

            _logger.LogInformation($"Phone number successfully updated for Admin ID: {adminId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the phone number.");
            throw;
        }
    }

}
