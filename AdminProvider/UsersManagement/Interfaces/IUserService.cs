using AdminProvider.UsersManagement.Models.DTOs;
using AdminProvider.UsersManagement.Models.Requests;
using System.Reflection.Metadata;

namespace AdminProvider.UsersManagement.Interfaces;

public interface IUserService
{
    Task<(List<UserDto> Users, int TotalCount, int CompanyCount)> GetAllUsers(int pageNumber, int pageSize);
    Task<List<UserDto>> GetUsersByQueryAsync(string searchQuery);
    Task<UserDetailsDto> GetUserAsync(string userId);
    Task<string> UpdateAdminNote(UserNoteUpdateRequest userNoteUpdateRequest);
    Task DeleteUserAsync(UserRequest userRequest);
}