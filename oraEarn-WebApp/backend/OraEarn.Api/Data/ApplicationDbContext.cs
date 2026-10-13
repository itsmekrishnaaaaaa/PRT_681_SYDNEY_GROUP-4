using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Expenses;
using OraEarn.Api.Jobs;
using OraEarn.Api.Notifications;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Shifts;
using OraEarn.Api.Users;

namespace OraEarn.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ActiveShiftSession> ActiveShiftSessions => Set<ActiveShiftSession>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<UserNotificationRead> UserNotificationReads => Set<UserNotificationRead>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Job>(entity =>
        {
            entity.HasIndex(j => j.UserId);
            entity.Property(j => j.HourlyRate).HasPrecision(18, 2);
            entity.HasOne(j => j.User)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Shift>(entity =>
        {
            entity.HasIndex(s => s.JobId);
            entity.HasIndex(s => s.WorkDate);
            entity.Property(s => s.TotalHours).HasPrecision(18, 2);
            entity.Property(s => s.TotalEarned).HasPrecision(18, 2);
            entity.HasOne(s => s.Job)
                .WithMany(j => j.Shifts)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ActiveShiftSession>(entity =>
        {
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasOne(s => s.User)
                .WithMany(u => u.ActiveShiftSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Job)
                .WithMany(j => j.ActiveShiftSessions)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasIndex(c => c.UserId);
            entity.HasOne(c => c.User)
                .WithMany(u => u.ExpenseCategories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PaymentMethod>(entity =>
        {
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(64);
        });

        builder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Date);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentMethod)
                .WithMany(p => p.Expenses)
                .HasForeignKey(e => e.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SavingsGoal>(entity =>
        {
            entity.HasIndex(g => g.UserId);
            entity.Property(g => g.TargetAmount).HasPrecision(18, 2);
            entity.Property(g => g.CurrentAmount).HasPrecision(18, 2);
            entity.Property(g => g.ManualAmount).HasPrecision(18, 2);
            entity.Property(g => g.AllocationPercentage).HasPrecision(5, 2);
            entity.HasOne(g => g.User)
                .WithMany(u => u.SavingsGoals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserNotificationRead>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.NotificationKey }).IsUnique();
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
