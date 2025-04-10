namespace AdminProvider.UsersManagement.Models.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
    public string CompanyName { get; set; }
    public string BusinessType { get; set; }
    public bool IsCompany { get; set; }
    public bool AdminNote { get; set; }
    public int OrderCount { get; set; }
}
