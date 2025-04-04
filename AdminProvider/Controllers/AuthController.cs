using Microsoft.AspNetCore.Mvc;
using AuthenticationProvider.Models.Requests;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using AdminProvider.ModeratorsManagement.Models.Requests;

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

        // Check if the user needs to change their password
        var moderators = _configuration.GetSection("Admins").Get<List<AdminEntity>>() ?? new List<AdminEntity>();

        // Find the specific admin, e.g., by AdminId or Email
        var admin = moderators.FirstOrDefault(m => m.Email == signInRequest.Email); // Replace 'someAdminId' with the actual AdminId you're checking

        if (admin == null)
        {
            return NotFound("Admin not found.");
        }

        // Check if the password is chosen
        if (!admin.PasswordChosen)
        {
            // Send the email (or user ID) so the frontend knows which user it is
            return BadRequest(new
            {
                message = "You need to change your password first.",
                redirectToChangePassword = true,
                adminId = admin.AdminId  // This identifies which admin needs to change their password
            });
        }

        // Continue with normal login or process...

        var signInResponse = await _signInService.SignInAsync(signInRequest);

        if (!signInResponse.Success)
        {
            return Unauthorized(new { Success = false, ErrorMessage = signInResponse.ErrorMessage });
        }

        // Log the full response
        _logger.LogInformation("Sign-in response: {@SignInResponse}", signInResponse);

        // Token is already set in cookie inside GenerateAccessToken
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
    public async Task<IActionResult> PasswordChange([FromBody] PasswordChangeRequest request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("❌ Bad request: Request body is null.");
                return BadRequest(new { success = false, message = "Uppgifter saknas." });
            }

            _logger.LogInformation($"✅ Received PasswordChangeRequest: Email = {request.AdminId}, Password = {request.Password}");

            await _adminService.PasswordChange(request);

            _logger.LogInformation("✅ Password change process completed successfully.");
            return Ok(new { success = true, message = "Lösenordet har ändrats." });  // Ensure JSON response
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

        // Call the service to check the authentication and role
        var authStatus = _accessTokenService.ValidateAccessToken();

        if (authStatus.IsAuthenticated)
        {
            return Ok(authStatus);
        }
        else
        {
            // If authentication fails, return unauthorized
            return Unauthorized(new { isAuthenticated = false, errorMessage = "Authentication failed or token is invalid." });
        }
    }
}
