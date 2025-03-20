using System.ComponentModel.DataAnnotations;

namespace GymManagmentSystem.Models
{
    public class MembershipPlan
    {
        public int MembershipId { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string PlanDescription { get; set; }
        public int MembershipPlanId { get; set; }
        public int DurationInMonths { get; set; }
        public decimal Price { get; set; } 
        public string Description { get; set; }  
        public bool IsActive { get; set; } = true; 
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
