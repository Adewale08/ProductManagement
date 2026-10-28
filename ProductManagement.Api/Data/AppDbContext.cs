using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Models;
namespace ProductManagement.Api.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Business> Businesses => Set<Business>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Business>().ToTable("Business");

        var product = modelBuilder.Entity<Product>();
        product.HasKey(p => p.Id);
        product.Property(p => p.Name).HasMaxLength(120).IsRequired();
        product.Property(p => p.Sku).HasMaxLength(40).IsRequired();
        product.HasIndex(p => p.Sku).IsUnique();
        product.Property(p => p.Category).HasMaxLength(80).IsRequired();
        product.Property(p => p.Price).HasPrecision(18, 2);
        product.Property(p => p.Description).HasMaxLength(500);
        
        var category = modelBuilder.Entity<Category>();
        category.HasKey(c => c.Id);
        category.Property(c => c.Name).HasMaxLength(100).IsRequired();
        category.Property(c => c.Description).HasMaxLength(250);

        var supplier = modelBuilder.Entity<Supplier>();
        supplier.HasKey(s => s.Id);
        supplier.Property(s => s.Name).HasMaxLength(120).IsRequired();
        supplier.Property(s => s.Email).HasMaxLength(254).IsRequired();
        supplier.Property(s => s.PhoneNumber).HasMaxLength(30);

        modelBuilder.Entity<Product>()
            .HasOne(p=> p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);
    }

}