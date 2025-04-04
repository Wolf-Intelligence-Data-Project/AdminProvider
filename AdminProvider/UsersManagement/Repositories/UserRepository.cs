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
    public async Task<(List<UserEntity>, int, int)> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _userDbContext.Users.CountAsync(); // Total users count
        var companyCount = await _userDbContext.Users.CountAsync(u => u.IsCompany); // Count of companies

        var users = await _userDbContext.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount, companyCount);
    }

    public async Task<UserEntity> GetByIdAsync(Guid userId)
    {
        if (userId == null)
        {
            _logger.LogError("The user does not exist.");
            throw new ArgumentNullException("Användaren finns inte.");
        }

        var user = await _userDbContext.Users
            .Include(u => u.Addresses)
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return user;
    }

    public async Task<List<UserEntity>> GetUsersByQueryAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogError("The search term is empty.");
            throw new ArgumentNullException("The search term is empty.");
        }

        // Return all matching users instead of just one
        var users = await _userDbContext.Users
            .Where(u => u.Email.Contains(searchTerm) ||
                        u.FullName.Contains(searchTerm) ||
                        u.CompanyName.Contains(searchTerm) ||
                        u.IdentificationNumber.Contains(searchTerm))
            .ToListAsync(); // Get all matching users

        return users;
    }

    public async Task<string> UpdateAdminNoteAsync(Guid userId, string adminNote)
    {
        try
        {
            var user = await _userDbContext.Set<UserEntity>().FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Användaren finns inte.");
            }

            user.AdminNote = adminNote;

            // Mark the specific property as modified
            _userDbContext.Entry(user).Property(u => u.AdminNote).IsModified = true;

            await _userDbContext.SaveChangesAsync();

            return user.AdminNote; // Return the updated entity
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user.");
            throw;
        }
    }

    public async Task DeleteOneAsync(Guid userId)
    {
        try
        {
            var user = await _userDbContext.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return;
            }

            _userDbContext.Users.Remove(user);
            await _userDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", userId);
            throw;
        }
    }
}
