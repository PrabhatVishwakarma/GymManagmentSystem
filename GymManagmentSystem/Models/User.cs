using Microsoft.AspNetCore.Identity;
using System;

namespace GymManagmentSystem.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Occupation { get; set; }
        public string ProfilePhotoUrl { get; set; } // Store base64 image or file path
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Role is handled by Identity framework
    }
}
