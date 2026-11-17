using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Application.Donors
{
    public class SetDonorAvailabilityRequest
    {
        // Nullable plus [Required] so a missing field is rejected,
        // instead of silently becoming false and opting the donor out.
        [Required]
        public bool? IsAvailable { get; set; }
    }
}