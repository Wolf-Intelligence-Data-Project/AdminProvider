using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var admin = await _profileService.GetModeratorAsync();

                return Ok(admin);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Ogiltig eller saknad access token.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Användaren hittades inte.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel inträffade när profilen skulle hämtas.");
                return StatusCode(500, "Ett oväntat fel inträffade.");
            }
        }
        [HttpPatch("change-password")]
        public async Task<IActionResult> PasswordChange([FromBody] PasswordChangeRequest request)
        {
            _logger.LogInformation("Incoming password change request: {@Request}", request);

            if (request == null)
            {
                return BadRequest("Felaktiga uppgifter. Försök igen."); // Invalid input
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid data received: {Errors}", errors);
                return BadRequest($"Ogiltiga uppgifter: {errors}");
            }

            try
            {
                await _profileService.PasswordChangeAsync(request);
                return Ok("Lösenordet har uppdaterats.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Verifieringsfel vid lösenordsändring.");
                return BadRequest(ex.Message); // User-friendly messages are already in Swedish
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett internt fel uppstod vid lösenordsändring.");
                return StatusCode(500, "Ett oväntat fel uppstod. Försök igen senare.");
            }
        }

    }
}
