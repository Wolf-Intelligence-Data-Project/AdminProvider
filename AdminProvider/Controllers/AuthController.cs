using Microsoft.AspNetCore.Mvc;
using AuthenticationProvider.Models.Requests;
using AdminProvider.ModeratorsManagement.Models.Responses;
using AdminProvider.ModeratorsManagement.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using AdminProvider.ModeratorsManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace AdminProvider.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly ISignInService _signInService;
    private readonly ISignOutService _signOutService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAccessTokenService accessTokenService, ISignInService signInService,ISignOutService signOutService, ILogger<AuthController> logger)
    {
        _accessTokenService = accessTokenService ?? throw new ArgumentNullException(nameof(accessTokenService));
        _signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
        _signOutService = signOutService ?? throw new ArgumentNullException(nameof(signOutService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        var signInResponse = await _signInService.SignInAsync(signInRequest);

        if (!signInResponse.Success)
        {
            return Unauthorized(new { Success = false, ErrorMessage = signInResponse.ErrorMessage });
        }
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

        var token = Request.Cookies["AccessToken"];
        _logger.LogWarning("Token from cookies: {Token}", token);

        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Åtkomsttoken saknas." });
        }

        var isAuthenticated = _accessTokenService.ValidateAccessToken(token);
        return Ok(new { isAuthenticated });
    }


}
