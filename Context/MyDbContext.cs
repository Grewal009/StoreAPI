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
        // Relationship: Customer -> OrderDetails (One Customer to Many OrderDetails)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Customer)
            .WithMany(c => c.OrderDetails)
            .HasForeignKey(od => od.CustomerId)
            .OnDelete(DeleteBehavior.Restrict); // Prevents multiple cascading paths

        // Relationship: Order -> OrderDetails (One Order to Many OrderDetails)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting Order also deletes its OrderDetails

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