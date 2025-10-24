using GymManagmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymManagmentSystem.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<EnquiryHistory> EnquiryHistories { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<MembersMembership> MembersMemberships { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure MembersMembership relationships
            builder.Entity<MembersMembership>()
                .HasOne(m => m.Enquiry)
                .WithMany()
                .HasForeignKey(m => m.EnquiryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MembersMembership>()
                .HasOne(m => m.MembershipPlan)
                .WithMany()
                .HasForeignKey(m => m.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure EnquiryHistory relationship
            builder.Entity<EnquiryHistory>()
                .HasOne(eh => eh.Enquiry)
                .WithMany()
                .HasForeignKey(eh => eh.EnquiryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            builder.Entity<MembersMembership>()
                .Property(m => m.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<MembersMembership>()
                .Property(m => m.PaidAmount)
                .HasPrecision(18, 2);

            builder.Entity<MembershipPlan>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);
        }
    }
}
