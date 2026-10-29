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
        category.HasIndex(c => c.BusinessId);
        // Principal index for the tenant-aware product foreign key in the migration.
        category.HasIndex(c => new { c.Id, c.BusinessId })
            .IsUnique()
            .HasFilter(null);
        category.HasOne(c => c.Business)
            .WithMany(b => b.Categories)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        var supplier = modelBuilder.Entity<Supplier>();
        supplier.HasKey(s => s.Id);
        supplier.Property(s => s.Name).HasMaxLength(120).IsRequired();
        supplier.Property(s => s.Email).HasMaxLength(254).IsRequired();
        supplier.Property(s => s.PhoneNumber).HasMaxLength(30);
        supplier.HasIndex(s => s.BusinessId);
        supplier.HasIndex(s => new { s.BusinessId, s.Email }).IsUnique();
        // Principal index for the tenant-aware product foreign key in the migration.
        supplier.HasIndex(s => new { s.Id, s.BusinessId })
            .IsUnique()
            .HasFilter(null);
        supplier.HasOne(s => s.Business)
            .WithMany(b => b.Suppliers)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p=> p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.CategoryRecord)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.Business)
            .WithMany()
            .HasForeignKey(order => order.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        var orderItem = modelBuilder.Entity<OrderItem>();
        orderItem.Property(item => item.UnitPrice).HasPrecision(18, 2);
        orderItem.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        orderItem.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureNewCatalogRecordsHaveOwners();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureNewCatalogRecordsHaveOwners();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureNewCatalogRecordsHaveOwners()
    {
        if (ChangeTracker.Entries<Supplier>().Any(entry =>
                entry.State == EntityState.Added && entry.Entity.BusinessId is null) ||
            ChangeTracker.Entries<Category>().Any(entry =>
                entry.State == EntityState.Added && entry.Entity.BusinessId is null))
        {
            throw new InvalidOperationException(
                "New suppliers and categories must belong to a business.");
        }
    }

}
