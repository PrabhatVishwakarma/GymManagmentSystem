using GymManagmentSystem.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagmentSystem.Models
{
    public class EnquiryHistory
    {
        [Key]
        public int HistoryId { get; set; }
        public int EnquiryId { get; set; }  
        public Enquiry Enquiry { get; set; }  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsWhatsappNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Occupation { get; set; }
        public EnquiryAction ActionTaken { get; set; } 
        public DateTime? MembershipTakenDate { get; set; } 
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
