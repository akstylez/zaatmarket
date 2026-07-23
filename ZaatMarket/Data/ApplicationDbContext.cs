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
    }
}