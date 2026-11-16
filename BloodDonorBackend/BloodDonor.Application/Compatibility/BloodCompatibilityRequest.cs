using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Compatibility
{
    public class BloodCompatibilityRequest
    {
        [Required]
        [EnumDataType(typeof(BloodGroup))]
        public BloodGroup? RecipientBloodGroup { get; set; }

        [Required]
        [EnumDataType(typeof(BloodComponent))]
        public BloodComponent? Component { get; set; }
    }
}