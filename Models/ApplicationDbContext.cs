using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

namespace HRStaffManagement.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Staff> Staff { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure additional model settings
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasIndex(e => e.StaffId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.StaffId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.StaffName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhotoPath).HasMaxLength(255);
            });
        }
    }
}