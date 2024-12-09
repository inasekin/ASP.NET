using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<CustomerPreference> CustomerPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.HasOne(e => e.Role)
                      .WithMany() 
                      .HasForeignKey("RoleId");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).HasMaxLength(100);
                entity.Property(r => r.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.FirstName).HasMaxLength(100);
                entity.Property(c => c.LastName).HasMaxLength(100);
                entity.Property(c => c.Email).HasMaxLength(200);

                entity.HasMany(c => c.PromoCodes)
                      .WithOne(pc => pc.Customer)
                      .HasForeignKey(pc => pc.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Preference>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<PromoCode>(entity =>
            {
                entity.Property(pc => pc.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(pc => pc.ServiceInfo)
                    .HasMaxLength(200);

                entity.HasOne(pc => pc.Preference)
                    .WithMany()
                    .HasForeignKey(pc => pc.PreferenceId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(pc => pc.Customer)
                    .WithMany(c => c.PromoCodes)
                    .HasForeignKey(pc => pc.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CustomerPreference>()
                .HasKey(cp => new { cp.CustomerId, cp.PreferenceId });

            modelBuilder.Entity<CustomerPreference>()
                .HasOne(cp => cp.Customer)
                .WithMany(c => c.CustomerPreferences)
                .HasForeignKey(cp => cp.CustomerId);

            modelBuilder.Entity<CustomerPreference>()
                .HasOne(cp => cp.Preference)
                .WithMany()
                .HasForeignKey(cp => cp.PreferenceId);
        }
    }
}
