using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.BloodVerification
{
    public class RejectBloodGroupRequest
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}