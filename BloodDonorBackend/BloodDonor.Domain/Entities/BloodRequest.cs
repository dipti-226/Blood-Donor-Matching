namespace BloodDonor.Domain.Entities
{
    public class BloodRequest
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public BloodGroup PatientBloodGroup { get; set; }
        public BloodComponent Component { get; set; }
        public int UnitsRequired { get; set; }
        public UrgencyLevel Urgency { get; set; }
        public BloodRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserId { get; set; }
        public string? CancellationReason { get; set; }
    }
}