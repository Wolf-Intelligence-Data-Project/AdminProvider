using AdminProvider.ModeratorsManagement.Models.DTOs;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services
{
    public interface IProfileService
    {
        Task<AdminDto> GetModeratorAsync();
    }
}