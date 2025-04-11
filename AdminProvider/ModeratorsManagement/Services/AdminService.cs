using AdminProvider.ModeratorsManagement.Interfaces.Services;
using System.Text.Json;
using System.Security.Claims;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;
using AdminProvider.ModeratorsManagement.Utillities;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json.Linq;
using AdminProvider.ModeratorsManagement.Data.Entities;

namespace AdminProvider.ModeratorsManagement.Services;

public class AdminService : IAdminService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IAdminService> _logger;
    private readonly string _settingsFilePath;
    private readonly ICustomPasswordHasher<AdminEntity> _customPasswordHasher;

    public AdminService(IConfiguration configuration, ILogger<IAdminService> logger, ICustomPasswordHasher<AdminEntity> customPasswordHasher)
    {
        _configuration = configuration;
        _logger = logger;
        _settingsFilePath = "appsettings.json"; // Adjust if needed
        _customPasswordHasher = customPasswordHasher;
    }

    public async Task<List<AdminDto>> GetModerators()
    {
        return _configuration.GetSection("Admins").Get<List<AdminDto>>() ?? new List<AdminDto>();
    }

    public async Task<AdminDto> AddModerator(AdminRequest request)
    {
        if (request == null)
        {
            _logger.LogError("Admin request is null.");
            throw new ArgumentNullException(nameof(request), "Admin request cannot be null.");
        }

        _logger.LogInformation("Adding new moderator with email: {Email}", request.Email);

        // Instantiate password hasher
        var passwordHasher = new CustomPasswordHasher();
        var hashedPassword = passwordHasher.HashPassword(request.Password);

        _logger.LogInformation("Password for {Email} has been hashed.", request.Email);

        // Create the new admin entity
        var newModerator = new AdminEntity
        {
            AdminId = Guid.NewGuid(),
            Email = request.Email,
            Role = request.Role,
            PasswordHash = hashedPassword,
            FullName = request.FullName,
            IdentificationNumber = request.IdentificationNumber,
            PhoneNumber = request.PhoneNumber,
            PasswordChosen = false,
        };

        _logger.LogInformation("New moderator created with AdminId: {AdminId}", newModerator.AdminId);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        // Read and update the appsettings.json
        string json = await File.ReadAllTextAsync(filePath);
        var jsonObj = JObject.Parse(json);

        // Ensure "Admins" is a JSON array
        if (jsonObj["Admins"] == null || jsonObj["Admins"].Type != JTokenType.Array)
        {
            jsonObj["Admins"] = new JArray();
        }

        var adminsArray = (JArray)jsonObj["Admins"];

        // Check if a moderator with the same email or identification number exists
        foreach (var admin in adminsArray)
        {
            if (admin["Email"]?.ToString() == request.Email)
            {
                _logger.LogError("Moderator with email {Email} already exists.", request.Email);
                throw new InvalidOperationException($"A moderator with the email {request.Email} already exists.");
            }

            if (admin["IdentificationNumber"]?.ToString() == request.IdentificationNumber)
            {
                _logger.LogError("Moderator with identification number {IdentificationNumber} already exists.", request.IdentificationNumber);
                throw new InvalidOperationException($"A moderator with the identification number {request.IdentificationNumber} already exists.");
            }
        }

        _logger.LogInformation("Current number of moderators before adding: {Count}", adminsArray.Count);

        // Convert the new moderator to JSON and add to array
        var newAdminJson = JObject.FromObject(newModerator);
        adminsArray.Add(newAdminJson);

        // Write the updated JSON back to the file
        await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));

        _logger.LogInformation("Successfully wrote updated JSON to appsettings.json.");

        // Read back the file to confirm changes
        string updatedJson = await File.ReadAllTextAsync(filePath);
        _logger.LogInformation("Updated appsettings.json content: {Json}", updatedJson);

        // Return the newly created admin as DTO
        return new AdminDto
        {
            AdminId = newModerator.AdminId,
            Email = newModerator.Email,
            Role = newModerator.Role,
            PhoneNumber = newModerator.PhoneNumber,
            IdentificationNumber = newModerator.IdentificationNumber,
            FullName = newModerator.FullName
        };
    }

    public async Task DeleteModerator(DeleteRequest request)
    {
        try
        {
            if (request != null)
            {
                var adminId = request.AdminId;
                _logger.LogInformation("DeleteModerator called with AdminId: {AdminId}", adminId);

                // Get the existing list of moderators from configuration
                var moderators = _configuration.GetSection("Admins").Get<List<AdminDto>>() ?? new List<AdminDto>();
                _logger.LogInformation("Fetched current list of moderators: {ModeratorsCount} moderators found.", moderators.Count);

                // Find the moderator to delete based on AdminId
                var moderatorToDelete = moderators.FirstOrDefault(m => m.AdminId.ToString() == adminId);
                if (moderatorToDelete != null)
                {
                    _logger.LogInformation("Moderator found: {Moderator}", moderatorToDelete);

                    // Remove the moderator from the list
                    moderators.Remove(moderatorToDelete);
                    _logger.LogInformation("Moderator {AdminId} has been removed from the list.", adminId);

                    // Update the appsettings.json file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                    // Read the current appsettings.json file
                    string json = await File.ReadAllTextAsync(filePath);
                    var jsonObj = JObject.Parse(json);

                    // Remove the moderator from the "Admins" section
                    jsonObj["Admins"] = JToken.FromObject(moderators);

                    // Write the updated content back to the file
                    await File.WriteAllTextAsync(filePath, jsonObj.ToString());
                    _logger.LogInformation("appsettings.json file updated with the new moderator list.");
                }
                else
                {
                    _logger.LogWarning("Moderator with AdminId: {AdminId} not found.", adminId);
                    throw new Exception("Moderator not found.");
                }
            }
            else
            {
                _logger.LogError("Delete request was null.");
                throw new ArgumentNullException(nameof(request), "Delete request cannot be null.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting moderator.");
            throw; // Re-throw the exception after logging
        }
    }

    public async Task PasswordChangeFirstTime(FirstPasswordChangeRequest request)
{
    if (request == null)
    {
        _logger.LogWarning("Password change request is null.");
        throw new ArgumentNullException(nameof(request), "Password change request cannot be null");
    }

    try
    {
        _logger.LogInformation($"Processing password change request for AdminId: {request.AdminId}");

        // 1️⃣ Load the current admins list from the config file
        string filePath = "appsettings.json";
        string json = await File.ReadAllTextAsync(filePath);

        var configRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        // Ensure "Admins" is an array
        if (!configRoot.ContainsKey("Admins") || !(configRoot["Admins"] is JArray))
        {
            _logger.LogError("The 'Admins' section in the configuration is missing or not an array.");
            throw new InvalidOperationException("Invalid configuration format: 'Admins' must be an array.");
        }

        var moderators = JsonConvert.DeserializeObject<List<AdminEntity>>(configRoot["Admins"].ToString());

        _logger.LogInformation($"Retrieved {moderators.Count} admins from configuration.");

        // 2️⃣ Find the admin
        var adminIdGuid = Guid.Parse(request.AdminId);
        var moderatorToUpdate = moderators.FirstOrDefault(p => p.AdminId == adminIdGuid);
      
        if (moderatorToUpdate == null)
        {
            _logger.LogWarning($"Admin with ID {adminIdGuid} not found.");
            throw new InvalidOperationException("Moderator not found");
        }
        if (moderatorToUpdate.PasswordChosen != null || moderatorToUpdate.PasswordChosen == false)
        {
            _logger.LogWarning($"It is not first time this admin is changing password");
            throw new InvalidOperationException("It is not first password change.");
        }
        _logger.LogInformation($"Admin {moderatorToUpdate.Email} found. Hashing new password...");

        if (request.Password != request.ConfirmPassword)
            {
                _logger.LogError($"Error during password change.");
                throw new InvalidOperationException("");
            }
        // 3️⃣ Update the password
        var newPasswordHash = _customPasswordHasher.HashPassword(request.Password);
        moderatorToUpdate.PasswordHash = newPasswordHash;
        moderatorToUpdate.PasswordChosen = true;

        _logger.LogInformation("Password hashed successfully. Saving new admin list...");

        // 4️⃣ Save back to the JSON file
        configRoot["Admins"] = moderators;
        string updatedJson = JsonConvert.SerializeObject(configRoot, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, updatedJson);

        _logger.LogInformation($"Password updated for Admin ID: {adminIdGuid}");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error during password change: {ex.Message}");
        throw;
    }
}


public bool VerifyPassword(AdminEntity admin, string password)
    {
        // If PasswordLastChangedAt is null, the user must change their password
        if (admin.PasswordChosen == null || admin.PasswordChosen == false)
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