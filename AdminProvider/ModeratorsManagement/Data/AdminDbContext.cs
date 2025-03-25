//using AdminProvider.UsersManagement.Data;
//using Microsoft.EntityFrameworkCore;

//namespace AdminProvider.ModeratorsManagement.Data;

//public class AdminDbContext : DbContext
//{
//    public DbSet<AdminEntity> Admins { get; set; }
//    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        modelBuilder.Entity<AdminEntity>()
//            .HasKey(k => k.AdminId);
//    }
//}
