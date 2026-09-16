using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class DeclareBloodGroupRequest
    {
        [Required]
        public BloodGroup BloodGroup { get; set; }
    }
}