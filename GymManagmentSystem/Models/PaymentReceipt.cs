using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class PaymentReceipt
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("paymentReceiptId")]
        public int PaymentReceiptId { get; set; }
        
        [Required]
        [StringLength(50)]
        [BsonElement("receiptNumber")]
        public string ReceiptNumber { get; set; } // e.g., "REC-2024-00001"
        
        [Required]
        [BsonElement("membersMembershipId")]
        public int MembersMembershipId { get; set; }
        
        [BsonIgnore]
        public MembersMembership MembersMembership { get; set; }
        
        [Required]
        [BsonElement("amountPaid")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal AmountPaid { get; set; }
        
        [StringLength(50)]
        [BsonElement("paymentMethod")]
        public string PaymentMethod { get; set; } // Cash, Card, UPI, etc.
        
        [StringLength(100)]
        [BsonElement("transactionId")]
        public string TransactionId { get; set; }
        
        [BsonElement("totalAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalAmount { get; set; }
        
        [BsonElement("previousPaid")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal PreviousPaid { get; set; }
        
        [BsonElement("remainingAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal RemainingAmount { get; set; }
        
        [StringLength(500)]
        [BsonElement("notes")]
        public string Notes { get; set; }
        
        [BsonElement("paymentDate")]
        public DateTime PaymentDate { get; set; }
        
        [StringLength(100)]
        [BsonElement("receivedBy")]
        public string ReceivedBy { get; set; } // Staff/Admin name
        
        [StringLength(200)]
        [BsonElement("memberName")]
        public string MemberName { get; set; }
        
        [StringLength(100)]
        [BsonElement("memberEmail")]
        public string MemberEmail { get; set; }
        
        [StringLength(20)]
        [BsonElement("memberPhone")]
        public string MemberPhone { get; set; }
        
        [StringLength(100)]
        [BsonElement("planName")]
        public string PlanName { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("emailSent")]
        public bool EmailSent { get; set; } = false;
        
        [BsonElement("emailSentAt")]
        public DateTime? EmailSentAt { get; set; }
        
        // Store the generated HTML content for the receipt
        [BsonElement("htmlContent")]
        public string HtmlContent { get; set; }
    }
}

