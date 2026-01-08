using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webovka.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "Customer";

        [Required] 
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required] 
        public string PhoneNumber { get; set; }

        [Required] 
        public string Address { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}