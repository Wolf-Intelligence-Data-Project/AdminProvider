namespace AdminProvider.OrdersManagement.Models.Requests;

public class SortCriteria
{
    public string SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
}