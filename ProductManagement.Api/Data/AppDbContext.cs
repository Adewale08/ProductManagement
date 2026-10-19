using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Models;
namespace ProductManagement.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();
        product.HasKey(p => p.Id);
        product.Property(p => p.Name).HasMaxLength(120).IsRequired();
        product.Property(p => p.Sku).HasMaxLength(40).IsRequired();
        product.HasIndex(p => p.Sku).IsUnique();
        product.Property(p => p.Category).HasMaxLength(80).IsRequired();
        product.Property(p => p.Price).HasPrecision(18, 2);
        product.Property(p => p.Description).HasMaxLength(500);
        
        var customer = modelBuilder.Entity<Customer>();
        customer.HasKey(c => c.Id);
        customer.Property(c => c.Name).HasMaxLength(120).IsRequired();
        customer.Property(c => c.Email).HasMaxLength(120).IsRequired();
        customer.Property(c => c.PhoneNumber).HasMaxLength(20);

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
    }

}