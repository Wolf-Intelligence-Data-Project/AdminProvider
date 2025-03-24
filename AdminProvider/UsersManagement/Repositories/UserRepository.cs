using AdminProvider.UsersManagement.Data;
using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdminProvider.UsersManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _userDbContext;
    private readonly ILogger<UserRepository> _logger;
    public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
    {
        _userDbContext = context;
        _logger = logger;
    }

    public async Task<List<UserEntity>> GetAllUsersAsync()
    {
        return await _userDbContext.Users.ToListAsync();
    }

    public async Task<UserEntity> GetByIdAsync(Guid userId)
    {
        if (userId == null)
        {
            _logger.LogError("The user does not exist.");
            throw new ArgumentNullException("Användaren finns inte.");
        }

        var user = await _userDbContext.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return user;
    }

    public async Task<UserEntity> GetByEmailAsync(string email)
    {
        if (email == null)
        {
            _logger.LogError("The user does not exist.");
            throw new ArgumentNullException("Användaren finns inte.");
        }

        var user = await _userDbContext.Users
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(x => x.Email == email);

        return user;
    }

    public async Task<UserEntity> UpdateAdminNoteAsync(Guid userId, string adminNote)
    {
        try
        {
            var existingUser = await _userDbContext.Set<UserEntity>().FindAsync(userId);
            if (existingUser == null)
            {
                throw new InvalidOperationException("Användaren finns inte.");
            }

            existingUser.AdminNote = adminNote;

            // Mark the specific property as modified
            _userDbContext.Entry(existingUser).Property(u => u.AdminNote).IsModified = true;

            await _userDbContext.SaveChangesAsync();

            return existingUser; // Return the updated entity
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user.");
            throw;
        }
    }

}
