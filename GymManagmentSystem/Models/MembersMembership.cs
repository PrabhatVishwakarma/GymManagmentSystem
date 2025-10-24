using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GymManagmentSystem.Models
{
    public class MembersMembership
    {
        public int MembersMembershipId { get; set; }
        public int EnquiryId { get; set; }
        public Enquiry Enquiry { get; set; }
        public int MembershipPlanId { get; set; } 
        public MembershipPlan MembershipPlan { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate => StartDate.AddMonths(MembershipPlan?.DurationInMonths ?? 0); 
        public decimal TotalAmount { get; set; }  
        public decimal PaidAmount { get; set; } = 0;  
        public decimal RemainingAmount => TotalAmount - PaidAmount;  
        public DateTime? NextPaymentDueDate { get; set; }
        public bool IsInactive { get; set; } = false;  // Admin can manually deactivate
        public bool IsActive => !IsInactive && EndDate > DateTime.UtcNow;  // Check both manual flag and expiry date  
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
