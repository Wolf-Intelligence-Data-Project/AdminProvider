using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Data.Entities;

public class AdminEntity
{
    [Key]
    public Guid AdminId { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    [Required]
    public string IdentificationNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
}
