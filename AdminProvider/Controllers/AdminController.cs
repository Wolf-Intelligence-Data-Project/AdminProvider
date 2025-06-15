using AdminProvider.ModeratorsManagement.Interfaces.Services;
using AdminProvider.ModeratorsManagement.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers;

[Authorize(Policy = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;
    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAdmins()
    {
        try
        {
            var admins = await _adminService.GetModerators();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpPost("add-mod")]
    public async Task<IActionResult> AddModerator([FromBody] AdminRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid admin request data.");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { message = "Validation failed", errors });
        }

        try
        {
            var admin = await _adminService.AddModerator(request);
            return Ok(new { newModerator = admin });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding admin/moderator.");
            return StatusCode(500, "Ett internt serverfel inträffade. Försök igen senare.");
        }
    }


     [HttpDelete("delete-mod")]
    public async Task<IActionResult> DeleteModerator(DeleteRequest request)
    {
        if (request == null)
        {
            return BadRequest("Invalid admin request data.");
        }
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { message = "Validation failed", errors });
        }
        try
        {          
            await _adminService.DeleteModerator(request);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding admin.");

            return StatusCode(500, "Internal server error. Please try again later.");
        }
    }
}
