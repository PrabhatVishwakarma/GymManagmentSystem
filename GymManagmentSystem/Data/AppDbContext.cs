using GymManagmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Enquiry> Enquiries { get; set; }
    public DbSet<EnquiryHistory> EnquiryHistories { get; set; }
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<MembersMembership> MembersMemberships { get; set; }
}
