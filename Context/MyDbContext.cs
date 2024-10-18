using Microsoft.EntityFrameworkCore;
using Store.Api.Entities;

namespace Product.Api;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Item> Items { get; set; }
    public DbSet<Menu> Menus { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Item => OrderDetails relationship too
        modelBuilder.Entity<OrderDetail>()
            .HasOne<Item>()
            .WithMany()
            .HasForeignKey(od => od.ItemId)
            .OnDelete(DeleteBehavior.Restrict); // Or any other behavior

        // Relationship: Customer -> Order (One Customer to Many Orders)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId);



    }

}