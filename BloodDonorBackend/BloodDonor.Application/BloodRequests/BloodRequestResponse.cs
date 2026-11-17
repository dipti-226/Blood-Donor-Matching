using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodRequests
{
    public class BloodRequestResponse
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public BloodGroup PatientBloodGroup { get; set; }
        public BloodComponent Component { get; set; }
        public int UnitsRequired { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public BloodRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}