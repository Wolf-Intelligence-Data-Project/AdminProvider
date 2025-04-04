using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Factories;
using AdminProvider.UsersManagement.Interfaces;
using AdminProvider.UsersManagement.Models.DTOs;
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

    public async Task<(List<UserDto> Users, int TotalCount, int CompanyCount)> GetAllUsers(int pageNumber, int pageSize)
    {
        var (users, totalCount, companyCount) = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);

        // Convert UserEntities to UsersDto within the service layer
        var usersDto = UsersDtoFactory.CreateList(users);

        return (usersDto, totalCount, companyCount);
    }

    public async Task<List<UserDto>> GetUsersByQueryAsync(string searchQuery)
    {
        // Get the list of UserEntity objects based on the search query
        var users = await _userRepository.GetUsersByQueryAsync(searchQuery);

        // Convert UserEntity objects to UsersDto
        var usersDto = users.Select(UsersDtoFactory.Create).ToList();

        return usersDto;
    }

    public async Task<UserDetailsDto> GetUserAsync(string userId)
    {
        if (Guid.TryParse(userId, out var userGuid))
        {
            var user = await _userRepository.GetByIdAsync(userGuid);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Convert the user entity to a UserDetailsDto
            var userDetailsDto = UserDetailsDtoFactory.Create(user);

            return userDetailsDto;
        }

        throw new ArgumentException("Invalid user ID format", nameof(userId));
    }

    public async Task<string> UpdateAdminNote(UserNoteUpdateRequest userNoteUpdateRequest)
    {
        if (userNoteUpdateRequest == null || userNoteUpdateRequest.UserId == null || userNoteUpdateRequest.AdminNote == null)
        {
            _logger.LogError("There is nothing to update.");
        }
        var userId = userNoteUpdateRequest.UserId;
        string adminNote = userNoteUpdateRequest.AdminNote;

        var updatedNote = await _userRepository.UpdateAdminNoteAsync(userId, adminNote);

        return updatedNote;
    }

    public async Task DeleteUserAsync(UserRequest userRequest)
    {
        if (userRequest == null)
        {
            throw new ArgumentNullException(nameof(userRequest), "User request cannot be null.");
        }

        if (!Guid.TryParse(userRequest.SearchQuery, out Guid userId))
        {
            throw new ArgumentException("Invalid User ID format.", nameof(userRequest.SearchQuery));
        }

        await _userRepository.DeleteOneAsync(userId);
    }
}
