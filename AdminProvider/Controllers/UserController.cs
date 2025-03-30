using AdminProvider.UsersManagement.Models.Requests;
using AdminProvider.UsersManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService userService, ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>A list of users.</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsers();
            if (users == null || users.Count == 0)
            {
                return NotFound(new { Message = "No users found." });
            }
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all users.");
            return StatusCode(500, new { Message = "An error occurred while retrieving users." });
        }
    }

    /// <summary>
    /// Retrieves a user by their email.
    /// </summary>
    /// <param name="userEmailRequest">The request containing the email of the user.</param>
    /// <returns>The user details.</returns>
    [HttpPost("email")]
    public async Task<IActionResult> GetUserByEmail([FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { Message = "Email cannot be empty." });
        }

        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving the user with email {email}.");
            return StatusCode(500, new { Message = "An error occurred while retrieving the user." });
        }
    }

    /// <summary>
    /// Updates the admin note for a specific user.
    /// </summary>
    /// <param name="userNoteUpdateRequest">The request containing the user ID and the new admin note.</param>
    /// <returns>A response indicating whether the update was successful.</returns>
    [HttpPatch("update-note")]
    public async Task<IActionResult> UpdateAdminNote([FromBody] UserNoteUpdateRequest userNoteUpdateRequest)
    {
        if (userNoteUpdateRequest == null)
        {
            return BadRequest(new { Message = "Request body cannot be null." });
        }

        if (string.IsNullOrWhiteSpace(userNoteUpdateRequest.AdminNote))
        {
            return BadRequest(new { Message = "Admin note cannot be empty." });
        }

        try
        {
            bool updateSuccess = await _userService.UpdateAdminNote(userNoteUpdateRequest);
            if (updateSuccess)
            {
                return Ok(new { Message = "Admin note updated successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to update admin note." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the admin note.");
            return StatusCode(500, new { Message = "An error occurred while updating the admin note." });
        }
    }

}
