//using AdminProvider.UsersManagement.Data.Entities;
//using AdminProvider.UsersManagement.Data;
//using AdminProvider.ModeratorsManagement.Data;
//using Microsoft.EntityFrameworkCore;
//using AdminProvider.ModeratorsManagement.Interfaces.Repositories;

//namespace AdminProvider.ModeratorsManagement.Repositories;

//public class AdminRepository : IAdminRepository
//{
//    private readonly ILogger<AdminRepository> _logger;
//    private readonly AdminDbContext _adminDbContext;
//    public AdminRepository(ILogger<AdminRepository> logger, AdminDbContext adminDbContext)
//    {
//        _logger = logger;
//        _adminDbContext = adminDbContext;
//    }

//    public async Task<UserEntity?> GetByEmailAsync(string email)
//    {
//        try
//        {
//            _logger.LogWarning(email);
//            return await _adminDbContext.Admins
//                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower()); // Normalize casing
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error retrieving user by email.");
//            throw;
//        }
//    }

//}
