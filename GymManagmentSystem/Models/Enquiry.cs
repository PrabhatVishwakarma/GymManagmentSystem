using System.ComponentModel.DataAnnotations;

namespace GymManagmentSystem.Models
{
    public class Enquiry
    {
        public int EnquiryId { get; set; } 
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }
        
        public bool IsWhatsappNumber { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; }
        
        [StringLength(50)]
        public string City { get; set; }
        
        [StringLength(10)]
        public string Gender { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(100)]
        public string Occupation { get; set; }
        
        [StringLength(100)]
        public string Createdby { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [StringLength(100)]
        public string UpdatedBy { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
