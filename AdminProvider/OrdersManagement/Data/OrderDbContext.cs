using AdminProvider.UsersManagement.Data;
using Microsoft.EntityFrameworkCore;
using AdminProvider.OrdersManagement.Data.Entities;

namespace AdminProvider.OrdersManagement.Data
{
    public class OrderDbContext : DbContext
    {
        public DbSet<OrderEntity> Orders { get; set; }

        public OrderDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
    }
}
