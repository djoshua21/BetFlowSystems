using BetFlowSystems.Models.DbModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class ApplicationDbContext : IdentityDbContext<Admin>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
        {
        }


        public DbSet<User> AppUsers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BetType> BetTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ---------------- USERS ----------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.UserID);

                entity.Property(u => u.IdNumber).IsRequired().HasMaxLength(13);
                entity.HasIndex(u => u.IdNumber).IsUnique();

                entity.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Surname).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Address).HasMaxLength(200);
                entity.Property(u => u.CreatedDate).IsRequired()
                      .HasDefaultValueSql("GETDATE()");
            });

            // ---------------- ACCOUNTS ----------------
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");

                entity.HasKey(a => a.AccountID);

                entity.Property(a => a.AccountStatus)
                      .IsRequired()
                      .HasConversion<String>().HasMaxLength(20);

                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(a => a.Balance)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(a => a.Description)
                      .HasMaxLength(200);

                entity.Property(a => a.CreatedDate)
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                // FK
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Accounts)
                      .HasForeignKey(a => a.UserID)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // ---------------- BET TYPES ----------------
            modelBuilder.Entity<BetType>(entity =>
            {
                entity.ToTable("BetTypes");

                entity.HasKey(b => b.BetTypeID);

                entity.Property(b => b.Sport).IsRequired().HasMaxLength(30);
                entity.Property(b => b.EventName).IsRequired().HasMaxLength(50);
                entity.Property(b => b.Description).HasMaxLength(255);
            });

            // ---------------- BETS ----------------
            modelBuilder.Entity<Bet>(entity =>
            {
                entity.ToTable("Bets");

                entity.HasKey(b => b.BetID);

                entity.Property(b => b.BetAmount).IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(b => b.PossibleWinAmount).IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(b => b.Result).IsRequired()
                      .HasConversion<String>().HasMaxLength(20);


                entity.Property(b => b.BetDate).IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.LastUpdatedDate).IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                // FK: Account
                entity.HasOne(b => b.Account)
                      .WithMany(a => a.Bets)
                      .HasForeignKey(b => b.AccountID)
                      .OnDelete(DeleteBehavior.SetNull);

                // FK: BetType
                entity.HasOne(b => b.BetType)
                      .WithMany(bt => bt.Bets)
                      .HasForeignKey(b => b.BetTypeID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- TRANSACTIONS ----------------
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(t => t.TransactionID);

                entity.Property(t => t.Amount).IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(t => t.TransactionType).IsRequired()
                      .HasConversion<string>().HasMaxLength(20);

                entity.Property(t => t.TransactionDate).IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                // FK: Account
                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountID)
                      .OnDelete(DeleteBehavior.SetNull);

                // FK: Bet (nullable)
                entity.HasOne(t => t.Bet)
                      .WithMany(b => b.Transactions)
                      .HasForeignKey(t => t.BetID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }


}
