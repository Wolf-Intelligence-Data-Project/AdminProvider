using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Models.Requests;

namespace AdminProvider.UsersManagement.Interfaces;

public interface IUserService
{
    Task<List<UserEntity>> GetAllUsers();
    Task<UserEntity> GetUserByEmailAsync(string email);
    Task<bool> UpdateAdminNote(UserNoteUpdateRequest userNoteUpdateRequest);
}
