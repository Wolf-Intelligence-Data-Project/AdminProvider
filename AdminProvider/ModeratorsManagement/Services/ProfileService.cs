using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace AdminProvider.ModeratorsManagement.Services;

public class ProfileService : IProfileService
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ProfileService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ProfileService(IAdminService adminService, ILogger<ProfileService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _adminService = adminService;
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



}
