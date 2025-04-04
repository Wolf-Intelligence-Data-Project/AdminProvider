using System.ComponentModel.DataAnnotations;

namespace AdminProvider.UsersManagement.Models.Requests;

public class UserRequest
{
    [Required]
    public string SearchQuery { get; set; }
}
