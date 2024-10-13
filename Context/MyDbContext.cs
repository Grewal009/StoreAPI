using Microsoft.EntityFrameworkCore;
using Store.Api.Entities;

namespace Product.Api;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Item> Items { get; set; }
    public DbSet<Menu> Menus { get; set; }

}