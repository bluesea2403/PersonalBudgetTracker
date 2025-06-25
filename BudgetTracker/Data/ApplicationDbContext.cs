using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<CategoryGroup> CategoryGroups { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<GoalContribution> GoalContributions { get; set; }

        // --- BẠN CẦN THÊM NGUYÊN PHẦN NÀY VÀO ---
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Quan trọng: Luôn gọi hàm base trước

            // Tắt hành vi xóa theo chuỗi (cascade delete) cho các mối quan hệ gây xung đột
            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Budget>()
        .HasOne(b => b.Category)
        .WithMany()
        .HasForeignKey(b => b.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RecurringTransaction>()
        .HasOne(r => r.Category)
        .WithMany()
        .HasForeignKey(r => r.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);
            // Thêm khối này vào bên trong OnModelCreating
            builder.Entity<FinancialGoal>()
                .HasMany(g => g.Contributions)
                .WithOne(c => c.FinancialGoal)
                .HasForeignKey(c => c.FinancialGoalId)
                .OnDelete(DeleteBehavior.Cascade); // Khi xóa mục tiêu, xóa luôn các khoản đóng góp
        }
        // --- KẾT THÚC PHẦN THÊM ---
    }
}