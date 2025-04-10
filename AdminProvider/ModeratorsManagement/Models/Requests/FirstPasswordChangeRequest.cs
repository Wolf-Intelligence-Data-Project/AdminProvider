using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class FirstPasswordChangeRequest
{
    [Required]
    public string AdminId { get; set; }
    [Required]
    public string Password { get; set; }
}
