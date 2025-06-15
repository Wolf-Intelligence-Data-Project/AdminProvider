using Microsoft.EntityFrameworkCore;
using AdminProvider.OrdersManagement.Data.Entities;

namespace AdminProvider.OrdersManagement.Data;

public class OrderDbContext : DbContext
{
    public DbSet<OrderEntity> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
}
