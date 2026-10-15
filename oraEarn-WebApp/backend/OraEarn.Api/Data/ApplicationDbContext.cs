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

    // Delete-rule policy:
    // - Cascade from ApplicationUser: rows that only make sense for that user (jobs, categories,
    //   goals, clock-in session, notification read state) go with the account.
    // - Restrict wherever a row is financial history: shifts (hours and pay) and expenses must not
    //   disappear because someone tidied up a job, category or payment method.
    // Because of the Restrict rules, deleting a job or category that still has shifts or expenses
    // fails at the database; the services do not yet turn that into a friendly 400. The cascade from
    // a user therefore only completes for users with no shifts or expenses - today that is the
    // rollback of a failed registration in AuthService, the only place a user is deleted.
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
            // WorkDate is indexed for the date-range filters and the 14-day visa window query.
            entity.HasIndex(s => s.JobId);
            entity.HasIndex(s => s.WorkDate);
            entity.Property(s => s.TotalHours).HasPrecision(18, 2);
            entity.Property(s => s.TotalEarned).HasPrecision(18, 2);

            // Restrict: deleting a job must not silently erase the earnings history logged against it.
            entity.HasOne(s => s.Job)
                .WithMany(j => j.Shifts)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ActiveShiftSession>(entity =>
        {
            // Unique: a person can only be clocked in once. This is the database-level guarantee
            // behind the check in ActiveShiftSessionService.ClockInAsync, which alone could be
            // bypassed by two requests arriving at the same time.
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasOne(s => s.User)
                .WithMany(u => u.ActiveShiftSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict: a job cannot be deleted while someone is clocked into it, otherwise clock-out
            // would have no job (and no hourly rate) to turn the session into a shift.
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

        // Shared lookup table introduced by the 3NF migration so the method name is stored once,
        // not repeated as free text on every expense. The unique name backs DbSeeder's
        // check-before-insert, so restarts never create duplicate methods.
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

            // Restrict on both: removing a category or a payment method must not delete the
            // expenses that budgets, summaries and analytics are calculated from.
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
            entity.Property(g => g.AllocationPercentage).HasPrecision(5, 2);
            entity.HasOne(g => g.User)
                .WithMany(u => u.SavingsGoals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserNotificationRead>(entity =>
        {
            // Notifications are generated on the fly, so only "user X has read key Y" is stored;
            // the unique pair means a key can only ever be recorded as read once per user.
            entity.HasIndex(r => new { r.UserId, r.NotificationKey }).IsUnique();
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
