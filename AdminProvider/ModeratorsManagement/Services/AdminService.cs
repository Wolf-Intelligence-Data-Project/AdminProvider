using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Interfaces.Utillities;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;
using AdminProvider.ModeratorsManagement.Utillities;
using Newtonsoft.Json;
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
        _settingsFilePath = "appsettings.json";
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

        var passwordHasher = new CustomPasswordHasher();
        var hashedPassword = passwordHasher.HashPassword(request.Password);

        _logger.LogInformation("Password for {Email} has been hashed.", request.Email);

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

        // Admins to a JSON array
        if (jsonObj["Admins"] == null || jsonObj["Admins"].Type != JTokenType.Array)
        {
            jsonObj["Admins"] = new JArray();
        }

        var adminsArray = (JArray)jsonObj["Admins"];

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

        string updatedJson = await File.ReadAllTextAsync(filePath);
        _logger.LogInformation("Updated appsettings.json content: {Json}", updatedJson);

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

                var moderators = _configuration.GetSection("Admins").Get<List<AdminDto>>() ?? new List<AdminDto>();
                _logger.LogInformation("Fetched current list of moderators: {ModeratorsCount} moderators found.", moderators.Count);

                var moderatorToDelete = moderators.FirstOrDefault(m => m.AdminId.ToString() == adminId);
                if (moderatorToDelete != null)
                {
                    _logger.LogInformation("Moderator found: {Moderator}", moderatorToDelete);

                    moderators.Remove(moderatorToDelete);
                    _logger.LogInformation("Moderator {AdminId} has been removed from the list.", adminId);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                    string json = await File.ReadAllTextAsync(filePath);
                    var jsonObj = JObject.Parse(json);

                    jsonObj["Admins"] = JToken.FromObject(moderators);

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
            throw;
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

        string filePath = "appsettings.json";
        string json = await File.ReadAllTextAsync(filePath);

        var configRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (!configRoot.ContainsKey("Admins") || !(configRoot["Admins"] is JArray))
        {
            _logger.LogError("The 'Admins' section in the configuration is missing or not an array.");
            throw new InvalidOperationException("Invalid configuration format: 'Admins' must be an array.");
        }

        var moderators = JsonConvert.DeserializeObject<List<AdminEntity>>(configRoot["Admins"].ToString());

        _logger.LogInformation($"Retrieved {moderators.Count} admins from configuration.");

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

        var newPasswordHash = _customPasswordHasher.HashPassword(request.Password);
        moderatorToUpdate.PasswordHash = newPasswordHash;
        moderatorToUpdate.PasswordChosen = true;

        _logger.LogInformation("Password hashed successfully. Saving new admin list...");

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

        if (admin.PasswordChosen == null || admin.PasswordChosen == false)
        {
            throw new InvalidOperationException("First login detected. Please change your password.");
        }

        return _customPasswordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);
    }
}