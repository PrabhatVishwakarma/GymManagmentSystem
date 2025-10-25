using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class MembersMembership
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("membersMembershipId")]
        public int MembersMembershipId { get; set; }
        
        [BsonElement("enquiryId")]
        public int EnquiryId { get; set; }
        
        [BsonIgnore]
        public Enquiry Enquiry { get; set; }
        
        [BsonElement("membershipPlanId")]
        public int MembershipPlanId { get; set; } 
        
        [BsonIgnore]
        public MembershipPlan MembershipPlan { get; set; } 
        
        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }
        
        [BsonElement("durationInMonths")]
        public int DurationInMonths { get; set; }
        
        [BsonIgnore]
        public DateTime EndDate => StartDate.AddMonths(DurationInMonths); 
        
        [BsonElement("totalAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalAmount { get; set; }  
        
        [BsonElement("paidAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal PaidAmount { get; set; } = 0;  
        
        [BsonIgnore]
        public decimal RemainingAmount => TotalAmount - PaidAmount;  
        
        [BsonElement("nextPaymentDueDate")]
        public DateTime? NextPaymentDueDate { get; set; }
        
        [BsonElement("isInactive")]
        public bool IsInactive { get; set; } = false;  // Admin can manually deactivate
        
        [BsonIgnore]
        public bool IsActive => !IsInactive && EndDate > DateTime.UtcNow;  // Check both manual flag and expiry date  
        
        [BsonElement("createdBy")]
        public string CreatedBy { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
