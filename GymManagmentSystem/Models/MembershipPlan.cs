using System.ComponentModel.DataAnnotations;

namespace GymManagmentSystem.Models
{
    public class MembershipPlan
    {
        public int MembershipPlanId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PlanName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PlanType { get; set; }
        
        [Range(1, 24, ErrorMessage = "Duration must be between 1 and 24 months")]
        public int DurationInMonths { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; } 
        
        [StringLength(500)]
        public string Description { get; set; }  
        
        public bool IsActive { get; set; } = true; 
        
        [StringLength(100)]
        public string CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [StringLength(100)]
        public string UpdatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
