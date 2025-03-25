using System.Security.Claims;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IAdminService
{
    List<AdminEntity> GetModerators();
    void AddModerator(AdminEntity newModerator);
    void DeleteModerator(string moderatorId, bool isConfirmed, ClaimsPrincipal user);
    void ResetPassword(string email, string newPassword);
    bool VerifyPassword(AdminEntity admin, string password);

}