using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.DTOs;

public class AdminDto
{
    [Required]
    public Guid AdminId { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Role { get; set; }  // "Admin" or "Moderator"
    [Required]
    public string IdentificationNumber { get; set; }
    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string FullName { get; set; }
}
