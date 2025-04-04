namespace AdminProvider.UsersManagement.Models.DTOs;

public class UserDetailsDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
    public string CompanyName { get; set; }
    public string BusinessType { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsCompany { get; set; }
    public bool IsVerified { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string AdminNote { get; set; }
    public List<AddressDto> Addresses { get; set; }
}
