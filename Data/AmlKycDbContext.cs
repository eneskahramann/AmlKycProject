using Microsoft.EntityFrameworkCore;
using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Data;

public class AmlKycDbContext : DbContext
{
    public AmlKycDbContext(DbContextOptions<AmlKycDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Sanction> Sanctions { get; set; }
    public DbSet<RiskLog> RiskLogs { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Account>().Property(a => a.Balance).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Transfer>().Property(t => t.Amount).HasColumnType("decimal(18,2)");
    }
}