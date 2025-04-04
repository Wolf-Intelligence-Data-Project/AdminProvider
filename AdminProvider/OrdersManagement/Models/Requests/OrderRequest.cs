using System.ComponentModel.DataAnnotations;

namespace AdminProvider.OrdersManagement.Models.Requests;

public class OrderRequest
{
    [Required]
    public string Id { get; set; }
}
