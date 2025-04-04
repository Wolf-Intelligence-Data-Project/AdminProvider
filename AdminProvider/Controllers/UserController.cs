using AdminProvider.UsersManagement.Factories;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Models.Requests;
using AdminProvider.UsersManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace AdminProvider.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>A list of users.</returns>
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
    {
        var (userDtos, totalCount, companyCount) = await _userService.GetAllUsers(pageNumber, pageSize);

        // Log the fetched data
        _logger.LogInformation("Fetched {UserCount} users, TotalCount: {TotalCount}, CompanyCount: {CompanyCount}",
                                userDtos.Count, totalCount, companyCount);

        var result = new
        {
            Users = userDtos,
            TotalCount = totalCount,
            CompanyCount = companyCount
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their email.
    /// </summary>
    /// <param name="userEmailRequest">The request containing the email of the user.</param>
    /// <returns>The user details in UserDto format.</returns>
    [HttpPost("get-users")]
    public async Task<IActionResult> GetUserByQuery([FromBody] UserRequest request)
    {
        _logger.LogInformation($"Received request: {JsonSerializer.Serialize(request)}");

        if (string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            return BadRequest(new { Message = "Search query cannot be empty." });
        }

        try
        {
            var userDtos = await _userService.GetUsersByQueryAsync(request.SearchQuery);

            if (userDtos == null || !userDtos.Any())
            {
                return NotFound(new { Message = "No users found." });
            }

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving users for search query {request.SearchQuery}.");
            return StatusCode(500, new { Message = "An error occurred while retrieving users." });
        }
    }

    [HttpPost("get-user-details")]
    public async Task<IActionResult> GetUserDetails([FromBody] UserRequest request)
    {
        _logger.LogInformation($"Received request: {JsonSerializer.Serialize(request)}");

        if (string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            return BadRequest(new { Message = "Search query cannot be empty." });
        }

        try
        {
            var userDetailsDto = await _userService.GetUserAsync(request.SearchQuery); // Get a list

            if (userDetailsDto == null)
            {
                return NotFound(new { Message = "No users found." });
            }

            return Ok(userDetailsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving user.");
            return StatusCode(500, new { Message = "An error occurred while retrieving users." });
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
            string updatedNote = await _userService.UpdateAdminNote(userNoteUpdateRequest);

            if (!string.IsNullOrEmpty(updatedNote))  // ✅ Corrected check
            {
                return Ok(new { updatedNote, Message = "Admin note updated successfully." });
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

    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteUser([FromBody] UserRequest userId)
    {
        if (userId == null)
        {
            return BadRequest(new { Message = "Användar-ID har inte angetts." });
        }
        try
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(new { Message = "Användaren har tagits bort." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while deleting the user.");
            return StatusCode(500, new { Message = "Ett fel uppstod vid borttagning av användaren." });
        }
    }
}
