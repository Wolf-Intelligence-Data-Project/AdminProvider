namespace AdminProvider.UsersManagement.Models.DTOs;

public class AddressDto
{
    public string StreetAndNumber { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Region { get; set; }
    public bool IsPrimary { get; set; }
}
