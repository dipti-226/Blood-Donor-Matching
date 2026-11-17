using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.DonorRequests
{
    public class DonorOwnRequestResponse
    {
        public Guid DonorRequestId { get; set; }
        public DonorRequestStatus Status { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public string HospitalArea { get; set; } = string.Empty;
        public string HospitalCity { get; set; } = string.Empty;
        public BloodGroup BloodGroupNeeded { get; set; }
        public BloodComponent Component { get; set; }
        public int UnitsRequired { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public string? SharedContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}