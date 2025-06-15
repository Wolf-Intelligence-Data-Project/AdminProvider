using Microsoft.AspNetCore.Mvc;
using AuthenticationProvider.Models.Requests;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using AdminProvider.ModeratorsManagement.Models.Requests;
using AdminProvider.ModeratorsManagement.Data.Entities;

namespace AdminProvider.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly ISignInService _signInService;
    private readonly ISignOutService _signOutService;
    private readonly IAdminService _adminService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAccessTokenService accessTokenService, ISignInService signInService, ISignOutService signOutService, IAdminService adminService, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
        _signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
        _signOutService = signOutService ?? throw new ArgumentNullException(nameof(signOutService));
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration;
    }

    /// <summary>
    /// Handles the sign-in process for the user.
    /// </summary>
    /// <param name="signInRequest">The sign-in request containing email and password.</param>
    /// <returns>A response indicating success or failure with a token on successful authentication.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> SignInAsync([FromBody] SignInRequest signInRequest)
    {
        if (signInRequest == null)
        {
            _logger.LogWarning("Sign-in request is null.");
            return BadRequest(new { Success = false, ErrorMessage = "Ogiltig inloggningsförfrågan." });
        }

        var moderators = _configuration.GetSection("Admins").Get<List<AdminEntity>>() ?? new List<AdminEntity>();

        var admin = moderators.FirstOrDefault(m => m.Email == signInRequest.Email);

        if (admin == null)
        {
            return NotFound("Admin not found.");
        }

        if (!admin.PasswordChosen)
        {
            return BadRequest(new
            {
                message = "You need to change your password first.",
                redirectToChangePassword = true,
                adminId = admin.AdminId 
            });
        }

        var signInResponse = await _signInService.SignInAsync(signInRequest);

        if (!signInResponse.Success)
        {
            return Unauthorized(new { Success = false, ErrorMessage = signInResponse.ErrorMessage });
        }

        _logger.LogInformation("Sign-in response: {@SignInResponse}", signInResponse);

        return Ok(signInResponse);
    }


    /// <summary>
    /// Logs out the currently authenticated user by revoking the access token.
    /// </summary>
    /// <returns>Returns a success message upon successful logout.</returns>
    /// <response code="200">Logout successful.</response>
    /// <response code="400">Access token is missing.</response>
    /// <response code="500">Internal server error during logout.</response>
    [HttpDelete("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["AccessToken"];

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Loggar ut utan token.");
        }

        var signOutSuccess = await _signOutService.SignOutAsync(token);

        if (!signOutSuccess)
        {
            _logger.LogError("Logout failed due to an internal error.");
            return StatusCode(500, new { message = "Utloggning misslyckades på grund av ett internt fel." });
        }

        Response.Cookies.Append("AccessToken", "", new CookieOptions
        {
            Expires = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm")).AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });

        _logger.LogInformation("User successfully logged out.");
        return Ok(new { message = "Utloggning lyckades." });
    }

    [HttpPatch("password-change")]
    public async Task<IActionResult> PasswordChange([FromBody] FirstPasswordChangeRequest request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("❌ Bad request: Request body is null.");
                return BadRequest(new { success = false, message = "Uppgifter saknas." });
            }

            await _adminService.PasswordChangeFirstTime(request);

            _logger.LogInformation("✅ Password change process completed successfully.");
            return Ok(new { success = true, message = "Lösenordet har ändrats." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Password change failed: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Ett fel uppstod vid ändring av lösenord." });
        }
    }

    /// <summary>
    /// Checks the authentication status of the currently logged-in user.
    /// </summary>
    /// <returns>Returns authentication and verification status.</returns>
    /// <response code="200">Returns authentication status.</response>
    /// <response code="400">Access token is missing.</response>
    [Authorize]
    [HttpGet("status")]
    public IActionResult GetAuthStatus()
    {
        if (Request.Cookies == null)
        {
            _logger.LogWarning("Request.Cookies is null.");
            return BadRequest(new { message = "Åtkomsttoken saknas." });
        }

        var authStatus = _accessTokenService.ValidateAccessToken();

        if (authStatus.IsAuthenticated)
        {
            return Ok(authStatus);
        }
        else
        {
            return Unauthorized(new { isAuthenticated = false, errorMessage = "Authentication failed or token is invalid." });
        }
    }
}
