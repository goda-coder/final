using final.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Entities;

namespace final.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<FingerprintLog> FingerprintLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;


    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<VisaCard> VisaCards { get; set; } = null!;
    public DbSet<BankAccount> BankAccounts { get; set; } = null!;
    public DbSet<BankToken> BankTokens { get; set; } = null!;
    //public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Dispute> Disputes { get; set; } = null!;
    public DbSet<TrustedContact> TrustedContacts { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Transaction>()
            .HasOne(t => t.Sender)
            .WithMany(u => u.SentTransactions)
            .HasForeignKey(t => t.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transaction>()
            .HasOne(t => t.Receiver)
            .WithMany(u => u.ReceivedTransactions)
            .HasForeignKey(t => t.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FingerprintLog>()
            .HasOne(f => f.User)
            .WithMany(u => u.FingerprintLogs)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.UserId).IsRequired().HasMaxLength(450);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.Amount).HasColumnType("decimal(18,2)");
            entity.Property(n => n.NewBalance).HasColumnType("decimal(18,2)");
            entity.Property(n => n.SenderPhone).HasMaxLength(20);
            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });
        builder.Entity<VisaCard>()
          .HasOne(v => v.Bank)
          .WithMany(b => b.VisaCards)
          .HasForeignKey(v => v.BankId)
    .      OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VisaCard>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BankAccount>()
            .HasOne(a => a.VisaCard)
            .WithMany(v => v.Accounts)
            .HasForeignKey(a => a.VisaCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BankAccount>()
            .Property(a => a.Balance)
            .HasColumnType("decimal(18,2)");

        builder.Entity<BankToken>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankToken>()
            .HasOne(t => t.BankAccount)
            .WithMany()
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BankToken>()
            .HasIndex(t => t.Token)
            .IsUnique();
    }
}