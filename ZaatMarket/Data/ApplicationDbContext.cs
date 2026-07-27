using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZaatMarket.Models;

namespace ZaatMarket.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<AiMessage> AiMessages { get; set; } // CHANGED
    public DbSet<Order> Orders { get; set; }
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<TelemetryEvent> TelemetryEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Existing Message Foreign Key Rules ---
        builder.Entity<Message>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Message>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);

        // --- NEW: Performance & Query Optimization Indexes ---

        // Creates a composite index optimized for loading product reviews sorted by newest first
        builder.Entity<Review>()
            .HasIndex(r => new { r.ProductId, r.CreatedAtUtc })
            .HasDatabaseName("IX_Reviews_ProductId_CreatedAtUtc");

        // Optimizes category filtering and marketplace product browsing
        builder.Entity<Product>()
            .HasIndex(p => p.Category)
            .HasDatabaseName("IX_Products_Category");
    }
}