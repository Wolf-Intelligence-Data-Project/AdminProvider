using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class StatusRequest
{
    [Required]
    public string Role { get; set; }
}
