using AdminProvider.ModeratorsManagement.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers
{
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

    }
}
