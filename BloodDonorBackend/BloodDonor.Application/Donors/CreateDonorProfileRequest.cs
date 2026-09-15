using BloodDonor.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BloodDonor.Application.Donors
{
    public class CreateDonorProfileRequest
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;
    }
}