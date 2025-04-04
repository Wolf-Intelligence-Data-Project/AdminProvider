using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Models.DTOs;

namespace AdminProvider.UsersManagement.Factories;

public static class UsersDtoFactory
{
    public static UserDto Create(UserEntity user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            IdentificationNumber = user.IdentificationNumber,
            CompanyName = user.CompanyName,
            BusinessType = user.BusinessType,
            IsCompany = user.IsCompany,
            AdminNote = !string.IsNullOrWhiteSpace(user.AdminNote) 
        };
    }
    public static List<UserDto> CreateList(IEnumerable<UserEntity> users)
    {
        return users.Select(Create).ToList(); 
    }
}
