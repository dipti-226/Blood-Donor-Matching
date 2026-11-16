using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class ApproveBloodGroupRequest
    {
        [EnumDataType(typeof(BloodGroup))]
        public BloodGroup VerifiedBloodGroup { get; set; }
    }
}