using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdminProvider.ModeratorsManagement.Models.Tokens;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AdminProvider.ModeratorsManagement.Models.Responses;

namespace AdminProvider.ModeratorsManagement.Services;

public class AccessTokenService : IAccessTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccessTokenService> _logger;
    private readonly IMemoryCache _memoryCache;

    private static readonly string TokenCacheKey = "AccessToken_"; // Prefix for the cache key
    private static readonly string BlacklistCacheKey = "Blacklist_"; // Prefix for the blacklist cache key
    private static readonly string IpCacheKey = "IpAddress_";

    private readonly List<string> _cacheKeys = new List<string>(); // Added to track cache keys

    public AccessTokenService(IConfiguration configuration, ILogger<AccessTokenService> logger, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    #region Main Methods

    public async Task<string> GenerateAccessToken(AdminEntity admin)
    {
        if (admin == null)
            throw new ArgumentNullException(nameof(admin), "Admin is not found.");

        RevokeAndBlacklistAccessToken(admin.AdminId.ToString()).Wait();

        var secretKey = _configuration["JwtAccess:Key"];
        var issuer = _configuration["JwtAccess:Issuer"];
        var audience = _configuration["JwtAccess:Audience"];

        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT configuration is missing.");

        var claims = new[] {
    new Claim("passwordChosen", admin.PasswordChosen.ToString()),
    new Claim(ClaimTypes.Role, admin.Role), // This is where the role is stored
    new Claim("adminId", admin.AdminId.ToString()),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var userIpInfo = GetUserIp();

        // Store token in memory cache
        _memoryCache.Set(TokenCacheKey + admin.AdminId, tokenString, TimeSpan.FromHours(1));
        _cacheKeys.Add(TokenCacheKey + admin.AdminId); // Track cache key

        // Bind IP/GUID to token (Stored with the same expiration)
        _memoryCache.Set(IpCacheKey + admin.AdminId, userIpInfo.IpAddress, TimeSpan.FromHours(1));
        _cacheKeys.Add(IpCacheKey + admin.AdminId); // Track IP cache key

        _logger.LogInformation($"Generated new access token for user {admin.FullName}.");

        // Now return HttpOnly cookie instead of just the token string
        _httpContextAccessor.HttpContext?.Response?.Cookies.Append("AccessToken", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.Now.AddHours(1)
        });

        // Extract role from claims
        string role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(role))
        {
            _logger.LogWarning("There is no role for this admin.");
        }
        // Now return the response with role and success message
        return role;
    }

    public AuthStatus ValidateAccessToken()
    {
        try
        {
            // Check if HttpContext is available
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogError("HttpContext is null.");
                return new AuthStatus
                {
                    IsAuthenticated = false,
                    ErrorMessage = "HttpContext is null"
                };
            }

            // Get token from cookies if not provided
            string token = _httpContextAccessor.HttpContext?.Request?.Cookies["AccessToken"] ?? string.Empty;

            // Check if token is missing or blacklisted
            if (string.IsNullOrEmpty(token) || !CheckBlacklist(token))
            {
                _logger.LogWarning("Token is either missing or blacklisted.");
                return new AuthStatus
                {
                    IsAuthenticated = false,
                    ErrorMessage = "Token is either missing or blacklisted"
                };
            }

            // Initialize JWT token handler and validation parameters
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JwtAccess:Issuer"],
                ValidAudience = _configuration["JwtAccess:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtAccess:Key"]))
            };

            // Validate the token
            var principal = handler.ValidateToken(token, validationParameters, out _);

            // Extract adminId from token claims
            var adminId = principal.Claims.FirstOrDefault(c => c.Type == "adminId")?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                _logger.LogWarning("AdminId not found in token claims.");
                return new AuthStatus
                {
                    IsAuthenticated = false,
                    ErrorMessage = "AdminId not found in token claims"
                };
            }

            // Perform IP validation
            var storedIp = _memoryCache.Get<string>(IpCacheKey + adminId);
            var currentIp = GetUserIp().IpAddress;
            bool isAuthenticated = storedIp == null || storedIp == currentIp;

            // Log the result
            _logger.LogInformation($"Token validation successful for Admin ID: {adminId}. IP check: {isAuthenticated}");

            return new AuthStatus
            {
                IsAuthenticated = isAuthenticated,
                Role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            };
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return new AuthStatus
            {
                IsAuthenticated = false,
                ErrorMessage = $"Token validation failed: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during token validation: {ex.Message}");
            return new AuthStatus
            {
                IsAuthenticated = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    public async Task RevokeAndBlacklistAccessToken(string adminId)
    {
        if (adminId == null)
        {
            _logger.LogWarning("Attempted to revoke token for a null admin.");
            return;
        }

        try
        {
            var tokenKey = TokenCacheKey + adminId;

            if (_memoryCache.TryGetValue(tokenKey, out var currentToken))
            {
                _memoryCache.Set(BlacklistCacheKey + currentToken.ToString(), new BlacklistedToken
                {
                    Token = currentToken.ToString(),
                    ExpirationTime = DateTime.Now.AddMinutes(75)
                }, TimeSpan.FromMinutes(75));

                _memoryCache.Remove(tokenKey);
                _memoryCache.Remove(IpCacheKey + adminId);

                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Response.Cookies.Delete("AccessToken");
                    _logger.LogInformation($"Access token cleared for admin {adminId}.");
                }
            }
            else
            {
                _logger.LogWarning($"No active token found for admin {adminId}. Token not revoked.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error revoking token for admin {adminId}.");
        }
    }

    #endregion

    #region Helper Methods

    private UserIpInfo GetUserIp()
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        // Handle loopback addresses for local dev
        if (ipAddress == "::1" || ipAddress == "127.0.0.1")
        {
            ipAddress = "127.0.0.1";
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            // Generate a GUID as fallback for missing IP
            ipAddress = Guid.NewGuid().ToString();
            _logger.LogInformation($"Generated GUID as fallback: {ipAddress}");
        }

        var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";

        return new UserIpInfo
        {
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
    public string GetAdminIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var adminIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "adminId");
            return adminIdClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error decoding token: {ex.Message}");
            return null;
        }
    }

    private bool CheckBlacklist(string token)
    {
        if (_memoryCache.TryGetValue(BlacklistCacheKey + token, out BlacklistedToken blacklistedToken))
        {
            if (blacklistedToken.ExpirationTime > TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")))
            {
                _logger.LogWarning("Token is blacklisted.");
                return false;  // Token is blacklisted
            }
            _memoryCache.Remove(BlacklistCacheKey + token);
        }
        return true;  // Token is valid
    }

    #region Utilities 
    public void CleanUpExpiredTokens()
    {
        var currentTime = DateTime.Now;
        foreach (var tokenKey in _cacheKeys)  // Use _cacheKeys to iterate
        {
            if (IsTokenExpired(tokenKey, currentTime))
            {
                _memoryCache.Remove(tokenKey);
                _memoryCache.Remove(IpCacheKey + tokenKey); // Clean up the associated IP/GUID
                _logger.LogInformation($"Expired token and IP/GUID removed for token: {tokenKey}");
            }
        }
    }

    private bool IsTokenExpired(string tokenKey, DateTime currentTime)
    {
        var token = _memoryCache.Get<string>(tokenKey);
        // Logic to check if the token is expired, for example by parsing the JWT expiration claim
        return token != null && IsJwtExpired(token, currentTime);
    }

    private bool IsJwtExpired(string token, DateTime currentTime)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null)
        {
            return false; // Invalid token
        }

        var expiration = jwtToken?.Payload?.Expiration?.ToString();
        if (DateTime.TryParse(expiration, out var expiryTime))
        {
            return currentTime > expiryTime;
        }

        return false;
    }
    #endregion

    #endregion
}