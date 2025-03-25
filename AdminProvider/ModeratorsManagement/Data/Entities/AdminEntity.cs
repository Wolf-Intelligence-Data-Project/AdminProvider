using System.ComponentModel.DataAnnotations;

public class AdminEntity
{
    [Key]
    public Guid AdminId { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public string Role { get; set; }  // "Admin" or "Moderator"
    [Required]
    public string IdentificationNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime? PasswordLastChangedAt { get; set; }
}
