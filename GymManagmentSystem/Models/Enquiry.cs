using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class Enquiry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("enquiryId")]
        public int EnquiryId { get; set; } 
        
        [Required]
        [StringLength(50)]
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        [BsonElement("lastName")]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        [BsonElement("email")]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        [StringLength(20)]
        [BsonElement("phone")]
        public string Phone { get; set; }
        
        [BsonElement("isWhatsappNumber")]
        public bool IsWhatsappNumber { get; set; }
        
        [StringLength(200)]
        [BsonElement("address")]
        public string Address { get; set; }
        
        [StringLength(50)]
        [BsonElement("city")]
        public string City { get; set; }
        
        [StringLength(10)]
        [BsonElement("gender")]
        public string Gender { get; set; }
        
        [BsonElement("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(100)]
        [BsonElement("occupation")]
        public string Occupation { get; set; }
        
        [BsonElement("isConverted")]
        public bool IsConverted { get; set; } = false;
        
        [BsonElement("convertedDate")]
        public DateTime? ConvertedDate { get; set; }
        
        [StringLength(100)]
        [BsonElement("createdby")]
        public string Createdby { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
