using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IProfileService
{
    Task<AdminDto> GetModeratorAsync();
    Task PasswordChangeAsync(PasswordChangeRequest request);
    Task EmailChangeAsync(EmailChangeRequest request);
    Task PhoneNumberChangeAsync(PhoneNumberChangeRequest request);

}