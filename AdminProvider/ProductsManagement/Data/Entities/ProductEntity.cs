using System;

namespace AdminProvider.ProductsManagement.Data.Entities;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public string CompanyName { get; set; }
    public string OrganizationNumber { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string BusinessType { get; set; }
    public decimal Revenue { get; set; }
    public int NumberOfEmployees { get; set; }
    public string CEO { get; set; }
    public DateTime? SoldUntil { get; set; } 
    public Guid? CustomerId { get; set; }
    public DateTime? ReservedUntil { get; set; }
}
