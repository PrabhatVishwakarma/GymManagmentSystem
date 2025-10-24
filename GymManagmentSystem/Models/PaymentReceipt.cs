using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagmentSystem.Models
{
    public class PaymentReceipt
    {
        public int PaymentReceiptId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } // e.g., "REC-2024-00001"
        
        [Required]
        public int MembersMembershipId { get; set; }
        
        [ForeignKey("MembersMembershipId")]
        public MembersMembership MembersMembership { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Cash, Card, UPI, etc.
        
        [StringLength(100)]
        public string TransactionId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousPaid { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public DateTime PaymentDate { get; set; }
        
        [StringLength(100)]
        public string ReceivedBy { get; set; } // Staff/Admin name
        
        [StringLength(200)]
        public string MemberName { get; set; }
        
        [StringLength(100)]
        public string MemberEmail { get; set; }
        
        [StringLength(20)]
        public string MemberPhone { get; set; }
        
        [StringLength(100)]
        public string PlanName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public bool EmailSent { get; set; } = false;
        
        public DateTime? EmailSentAt { get; set; }
        
        // Store the generated HTML content for the receipt
        public string HtmlContent { get; set; }
    }
}

