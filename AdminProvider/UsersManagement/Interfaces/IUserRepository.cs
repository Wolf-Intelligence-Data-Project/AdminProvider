using AdminProvider.UsersManagement.Data.Entities;

namespace AdminProvider.UsersManagement.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserEntity>> GetAllUsersAsync();

        Task<UserEntity> GetByIdAsync(Guid userId);
        Task<UserEntity> GetByEmailAsync(string email);
        Task<UserEntity> UpdateAdminNoteAsync(Guid userId, string adminNote);
    }
}
