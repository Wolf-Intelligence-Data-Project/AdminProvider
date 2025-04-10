using AdminProvider.ModeratorsManagement.Data.Entities;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;
namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IAdminService
{
    Task <List<AdminDto>> GetModerators();
    Task<AdminDto> AddModerator(AdminRequest request);
    Task DeleteModerator(DeleteRequest request);

    Task PasswordChangeFirstTime(FirstPasswordChangeRequest request);
    //void ResetPassword(string email, string newPassword);

    bool VerifyPassword(AdminEntity admin, string password);

}