using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodRequests
{
    public class CreateBloodRequestRequest
    {
        [Required]
        [EnumDataType(typeof(BloodGroup))]
        public BloodGroup? PatientBloodGroup { get; set; }

        [Required]
        [EnumDataType(typeof(BloodComponent))]
        public BloodComponent? Component { get; set; }

        [Range(1, 50)]
        public int UnitsRequired { get; set; }

        [Required]
        [EnumDataType(typeof(UrgencyLevel))]
        public UrgencyLevel? Urgency { get; set; }
    }
}