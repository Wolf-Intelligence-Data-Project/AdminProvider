using System.ComponentModel.DataAnnotations;

namespace AdminProvider.UsersManagement.Models.Requests;

public class UserNoteUpdateRequest
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string AdminNote { get; set; }

}
