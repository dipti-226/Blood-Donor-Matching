using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.DonorRequests
{
    public class AcceptDonorRequestRequest
    {
        // Format check only (digits, optional leading +, 10 to 15 digits). The number is NOT verified.
        [Required]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Contact number must be 10 to 15 digits with an optional leading +.")]
        public string ContactNumber { get; set; } = string.Empty;
    }
}