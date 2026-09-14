using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.Auth
{
    public class LoginUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}