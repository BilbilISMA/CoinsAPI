using System.ComponentModel.DataAnnotations;

namespace FraktonCoins.Models.DTOs.Requests
{
    public class UserRegistrationDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}