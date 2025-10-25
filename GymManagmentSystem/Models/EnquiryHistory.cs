using GymManagmentSystem.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GymManagmentSystem.Models
{
    public class EnquiryHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("historyId")]
        public int HistoryId { get; set; }
        
        [BsonElement("enquiryId")]
        public int EnquiryId { get; set; }  
        
        [BsonIgnore]
        public Enquiry Enquiry { get; set; }  
        
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        
        [BsonElement("lastName")]
        public string LastName { get; set; }
        
        [BsonElement("email")]
        public string Email { get; set; }
        
        [BsonElement("phone")]
        public string Phone { get; set; }
        
        [BsonElement("isWhatsappNumber")]
        public bool IsWhatsappNumber { get; set; }
        
        [BsonElement("address")]
        public string Address { get; set; }
        
        [BsonElement("city")]
        public string City { get; set; }
        
        [BsonElement("gender")]
        public string Gender { get; set; }
        
        [BsonElement("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        
        [BsonElement("occupation")]
        public string Occupation { get; set; }
        
        [BsonElement("actionTaken")]
        public EnquiryAction ActionTaken { get; set; } 
        
        [BsonElement("membershipTakenDate")]
        public DateTime? MembershipTakenDate { get; set; } 
        
        [BsonElement("modifiedBy")]
        public string ModifiedBy { get; set; }
        
        [BsonElement("modifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
