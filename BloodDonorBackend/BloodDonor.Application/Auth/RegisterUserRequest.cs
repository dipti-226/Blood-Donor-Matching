using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.Auth
{
    public class RegisterUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public string RequestedRole { get; set; } = string.Empty;
    }
}