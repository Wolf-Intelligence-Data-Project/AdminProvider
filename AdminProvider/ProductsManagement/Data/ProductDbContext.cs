using Microsoft.EntityFrameworkCore;
using AdminProvider.ProductsManagement.Data.Entities;

namespace AdminProvider.ProductsManagement.Data;

public class ProductDbContext : DbContext
{
    // DbSet for ProductEntity table
    public DbSet<ProductEntity> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    // OnModelCreating is optional if you are not managing tables but just querying
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            // Set primary key
            entity.HasKey(e => e.ProductId);

            // Define properties and required constraints
            entity.Property(e => e.CompanyName).IsRequired();
            entity.Property(e => e.OrganizationNumber).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.PostalCode).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.PhoneNumber).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.BusinessType).IsRequired();
            entity.Property(e => e.NumberOfEmployees).IsRequired();
            entity.Property(e => e.CEO).IsRequired();
            entity.Property(e => e.CustomerId).IsRequired(false);
            entity.Property(e => e.SoldUntil).IsRequired(false);
            entity.Property(e => e.ReservedUntil).IsRequired(false);

            // Explicit mapping for Revenue with precision and scale
            entity.Property(e => e.Revenue)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // Ensure it uses decimal(18,2) in the database
        });
    }
}
