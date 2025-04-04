using AdminProvider.UsersManagement.Data.Entities;

namespace AdminProvider.UsersManagement.Interfaces;

public interface IUserRepository
{
    Task<(List<UserEntity>, int, int)> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserEntity> GetByIdAsync(Guid userId);
    Task<List<UserEntity>> GetUsersByQueryAsync(string searchTerm);
    Task<string> UpdateAdminNoteAsync(Guid userId, string adminNote);
    Task DeleteOneAsync(Guid userId);
}
