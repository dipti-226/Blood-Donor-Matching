using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class ResubmitBloodGroupRequest
    {
        [EnumDataType(typeof(BloodGroup))]
        public BloodGroup BloodGroup { get; set; }
    }
}