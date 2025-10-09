using InstantSyncBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstantSyncBackend.Persistence.DbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionQueue> TransactionQueues { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TransactionQueue>()
            .HasOne(tq => tq.Transaction)
            .WithMany()
            .HasForeignKey(tq => tq.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add unique constraint for account number
        builder.Entity<Account>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique();

        // Add index for transaction reference
        builder.Entity<Transaction>()
            .HasIndex(t => t.TransactionReference)
            .IsUnique();

        // Add indexes for queue processing
        builder.Entity<TransactionQueue>()
            .HasIndex(tq => new { tq.IsProcessing, tq.ScheduledProcessTime, tq.Priority });
    }
}
