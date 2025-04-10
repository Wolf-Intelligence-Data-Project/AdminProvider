using AdminProvider.ModeratorsManagement.Data.Entities;
using AdminProvider.ModeratorsManagement.Models.DTOs;
using AdminProvider.ModeratorsManagement.Models.Requests;
using Newtonsoft.Json.Linq;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services
{
    public interface IProfileService
    {
        Task<AdminDto> GetModeratorAsync();
        Task PasswordChangeAsync(PasswordChangeRequest request);

    }
}