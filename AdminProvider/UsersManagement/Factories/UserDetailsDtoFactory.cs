using AdminProvider.UsersManagement.Data.Entities;
using AdminProvider.UsersManagement.Models.DTOs;

namespace AdminProvider.UsersManagement.Factories;

public static class UserDetailsDtoFactory
{
    public static UserDetailsDto Create(UserEntity user)
    {
        return new UserDetailsDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            IdentificationNumber = user.IdentificationNumber,
            CompanyName = user.CompanyName,
            BusinessType = user.BusinessType,
            PhoneNumber = user.PhoneNumber,
            IsVerified = user.IsVerified,
            IsCompany = user.IsCompany,
            RegisteredAt = user.RegisteredAt,
            AdminNote = user.AdminNote,
            Addresses = user.Addresses.Select(a => new AddressDto
            {
                StreetAndNumber = a.StreetAndNumber,
                City = a.City,
                PostalCode = a.PostalCode,
                Region = a.Region,
                IsPrimary = a.IsPrimary
            }).ToList()
        };
    }
}
