using System;
using System.ComponentModel.DataAnnotations;

namespace GymManagmentSystem.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } // Email, WhatsApp, MembershipCreated, MembershipUpgraded, PaymentReceived, etc.
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        public string EntityType { get; set; } // Enquiry, Member, User, etc.
        
        public int? EntityId { get; set; } // ID of the related entity
        
        public string RecipientName { get; set; }
        
        public string RecipientContact { get; set; } // Email or Phone
        
        public string MessageContent { get; set; } // Full message content if applicable
        
        public bool IsSuccessful { get; set; } = true;
        
        public string ErrorMessage { get; set; }
        
        [StringLength(100)]
        public string PerformedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

