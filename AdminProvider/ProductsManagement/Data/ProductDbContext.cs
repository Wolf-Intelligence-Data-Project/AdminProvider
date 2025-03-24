using AdminProvider.ProductsManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminProvider.ProductsManagement.Data;

public class ProductDbContext : DbContext
{
    // DbSet for ProductEntity table
    public DbSet<ProductEntity> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products"); // Specify the table name in the database
            entity.HasKey(e => e.ProductId); // Define ProductId as primary key

            // Define other properties' column mappings, if necessary
            entity.Property(e => e.CompanyName).IsRequired();
            entity.Property(e => e.OrganizationNumber).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.PostalCode).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.PhoneNumber).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.BusinessType).IsRequired();
            entity.Property(e => e.Revenue).IsRequired();
            entity.Property(e => e.NumberOfEmployees).IsRequired();
            entity.Property(e => e.CEO).IsRequired();
            entity.Property(e => e.CustomerId).IsRequired(false);
            entity.Property(e => e.SoldUntil).IsRequired(false);
            entity.Property(e => e.ReservedUntil).IsRequired(false);
        });
    }
}