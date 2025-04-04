using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class DeleteRequest
{
    [Required]
    public string AdminId { get; set; }
}
