using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.BloodRequests
{
    public class CancelBloodRequestRequest
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}