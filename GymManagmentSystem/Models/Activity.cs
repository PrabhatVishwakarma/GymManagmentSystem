using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("activityId")]
        public int ActivityId { get; set; }
        
        [Required]
        [StringLength(100)]
        [BsonElement("activityType")]
        public string ActivityType { get; set; } // Email, WhatsApp, MembershipCreated, MembershipUpgraded, PaymentReceived, etc.
        
        [Required]
        [StringLength(500)]
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("entityType")]
        public string EntityType { get; set; } // Enquiry, Member, User, etc.
        
        [BsonElement("entityId")]
        public int? EntityId { get; set; } // ID of the related entity
        
        [BsonElement("recipientName")]
        public string RecipientName { get; set; }
        
        [BsonElement("recipientContact")]
        public string RecipientContact { get; set; } // Email or Phone
        
        [BsonElement("messageContent")]
        public string MessageContent { get; set; } // Full message content if applicable
        
        [BsonElement("isSuccessful")]
        public bool IsSuccessful { get; set; } = true;
        
        [BsonElement("errorMessage")]
        public string ErrorMessage { get; set; }
        
        [StringLength(100)]
        [BsonElement("performedBy")]
        public string PerformedBy { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

