using System.ComponentModel.DataAnnotations;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Hospitals
{
    public class UpdateHospitalStatusRequest
    {
        [Required]
        public HospitalStatus Status { get; set; }
    }
}