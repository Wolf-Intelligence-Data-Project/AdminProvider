using AdminProvider.ModeratorsManagement.Interfaces.Services;
using System.Text.Json;
using System.Security.Claims;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;

namespace AdminProvider.ModeratorsManagement.Services;

public class AdminService : IAdminService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminService> _logger;
    private readonly string _settingsFilePath;
    private readonly ICustomPasswordHasher<AdminEntity> _customPasswordHasher;

    public AdminService(IConfiguration configuration, IEmailService emailService, ILogger<AdminService> logger, ICustomPasswordHasher<AdminEntity> customPasswordHasher)
    {
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
        _settingsFilePath = "appsettings.json"; // Adjust if needed
        _customPasswordHasher = customPasswordHasher;
    }

    public List<AdminEntity> GetModerators()
    {
        return _configuration.GetSection("Admins").Get<List<AdminEntity>>() ?? new List<AdminEntity>();
    }

    public void AddModerator(AdminEntity newModerator)
    {
        var admins = GetModerators();
        //var requestingAdmin = admins.FirstOrDefault(a => a.Email == requestingAdminEmail);

        //if (requestingAdmin == null || requestingAdmin.Role != "Admin")
        //{
        //    throw new UnauthorizedAccessException("Only Admins can add new users.");
        //}

        //if (admins.Any(a => a.Email == newModerator.Email))
        //{
        //    throw new InvalidOperationException("Admin with this email already exists.");
        //}

        // Generate valid temporary password using the updated method
        string tempPassword = GenerateRandomPassword();

        // Send password to user's email via Brevo (or any email service)
        _emailService.SendTemporaryPasswordEmail(newModerator.Email, tempPassword);

        // Hash and store password
        newModerator.PasswordHash = _customPasswordHasher.HashPassword(tempPassword);

        // Ensure that the user must change the password on first login
        newModerator.PasswordLastChangedAt = null;

        admins.Add(newModerator);
        SaveModerators(admins);

        _logger.LogInformation($"Moderator {newModerator.Email} added. Temporary password sent securely via email.");
    }

    public void DeleteModerator(string moderatorId, bool isConfirmed, ClaimsPrincipal user)
    {
        if (!isConfirmed)
        {
            throw new InvalidOperationException("Moderator deletion not confirmed.");
        }

        var admins = GetModerators();

        //// Get the requesting admin's email from the JWT claims
        //var requestingAdminEmail = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Id)?.Value;

        //if (string.IsNullOrEmpty(requestingAdminEmail))
        //{
        //    throw new UnauthorizedAccessException("Admin email could not be found in the access token.");
        //}

        //var requestingAdmin = admins.FirstOrDefault(a => a.Email == requestingAdminEmail);

        //if (requestingAdmin == null || requestingAdmin.Role != "Moderator")
        //{
        //    throw new UnauthorizedAccessException("Only Admins can delete users.");
        //}

        var moderatorToDelete = admins.FirstOrDefault(a => a.Email == moderatorId && a.Role == "Moderator");
        if (moderatorToDelete == null)
        {
            throw new InvalidOperationException("Moderator with this email does not exist.");
        }

        admins.Remove(moderatorToDelete);
        SaveModerators(admins);

        //_logger.LogInformation($"Moderator {moderatorEmail} deleted successfully by {requestingAdminEmail}.");
    }


    public void ResetPassword(string email, string newPassword)
    {
        var admins = GetModerators();
        var user = admins.FirstOrDefault(a => a.Email == email);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Hash new password and update record
        user.PasswordHash = _customPasswordHasher.HashPassword(newPassword);
        user.PasswordLastChangedAt = DateTime.UtcNow; // Mark first-time password change

        SaveModerators(admins);

        _logger.LogInformation($"User {email} has successfully changed their password.");
    }

    public bool VerifyPassword(AdminEntity admin, string password)
    {
        // If PasswordLastChangedAt is null, the user must change their password
        if (admin.PasswordLastChangedAt == null)
        {
            throw new InvalidOperationException("First login detected. Please change your password.");
        }

        // Use the custom password hasher to verify the password
        return _customPasswordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);
    }


    private void SaveModerators(List<AdminEntity> admins)
    {
        try
        {
            var appSettings = File.ReadAllText(_settingsFilePath);
            var jsonSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(appSettings);

            jsonSettings["Admins"] = admins;
            File.WriteAllText(_settingsFilePath, JsonSerializer.Serialize(jsonSettings, new JsonSerializerOptions { WriteIndented = true }));

            _logger.LogInformation("Admin data successfully updated in appsettings.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update admin data.");
            throw;
        }
    }

    private string GenerateRandomPassword()
    {
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%";

        Random random = new();

        // Ensure at least one character from each set (lowercase, uppercase, digits, special chars)
        var password = new List<char>
    {
        lowercaseChars[random.Next(lowercaseChars.Length)],
        uppercaseChars[random.Next(uppercaseChars.Length)],
        digits[random.Next(digits.Length)],
        specialChars[random.Next(specialChars.Length)]
    };

        // Fill the rest of the password length with random characters from all sets
        const int passwordLength = 12; // You can change this to fit your needs
        string allChars = lowercaseChars + uppercaseChars + digits + specialChars;

        for (int i = password.Count; i < passwordLength; i++)
        {
            password.Add(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle the list to ensure randomness
        password = password.OrderBy(c => random.Next()).ToList();

        return new string(password.ToArray());
    }
}