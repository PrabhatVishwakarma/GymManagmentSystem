using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class MembershipPlan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("membershipPlanId")]
        public int MembershipPlanId { get; set; }
        
        [Required]
        [StringLength(100)]
        [BsonElement("planName")]
        public string PlanName { get; set; }
        
        [Required]
        [StringLength(50)]
        [BsonElement("planType")]
        public string PlanType { get; set; }
        
        [Range(1, 24, ErrorMessage = "Duration must be between 1 and 24 months")]
        [BsonElement("durationInMonths")]
        public int DurationInMonths { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } 
        
        [StringLength(500)]
        [BsonElement("description")]
        public string Description { get; set; }  
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true; 
        
        [StringLength(100)]
        [BsonElement("createdBy")]
        public string CreatedBy { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
