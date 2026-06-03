using Microsoft.EntityFrameworkCore;
using QuantIA.Models;

namespace QuantIA.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<InstallmentGroup> InstallmentGroups { get; set; }
    public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
    public DbSet<AiConfig> AiConfigs { get; set; }
    public DbSet<AiMessage> AiMessages { get; set; }
    public DbSet<FinancialProfile> FinancialProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Direction)
            .HasConversion<string>();

        modelBuilder.Entity<AiConfig>()
            .Property(c => c.Provider)
            .HasConversion<string>();

        modelBuilder.Entity<AiMessage>()
            .Property(m => m.Provider)
            .HasConversion<string>();

        // Relacionamento 1:1 entre User e FinancialProfile (unicidade por usuário)
        modelBuilder.Entity<FinancialProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<FinancialProfile>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}