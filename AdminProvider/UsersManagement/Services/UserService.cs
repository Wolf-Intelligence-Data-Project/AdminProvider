using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Models.Requests;

namespace AdminProvider.UsersManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserEntity>> GetAllUsers()
    {

        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<UserEntity> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user;
    }

    public async Task<bool> UpdateAdminNote(UserNoteUpdateRequest userNoteUpdateRequest)
    {
        var user = await _userRepository.GetByIdAsync(userNoteUpdateRequest.UserId);
        if (userNoteUpdateRequest == null || userNoteUpdateRequest.UserId == null || userNoteUpdateRequest.AdminNote == null)
        {
            _logger.LogError("There is nothing to update.");
        }
        var userId = userNoteUpdateRequest.UserId;
        string adminNote = userNoteUpdateRequest.AdminNote;

        await _userRepository.UpdateAdminNoteAsync(userId, adminNote);

        return true;
    }
}
