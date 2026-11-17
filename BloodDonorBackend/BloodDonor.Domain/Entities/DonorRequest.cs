namespace BloodDonor.Domain.Entities
{
    public class DonorRequest
    {
        public Guid Id { get; set; }
        public Guid BloodRequestId { get; set; }
        public Guid DonorProfileId { get; set; }
        public DonorRequestStatus Status { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        // Entered by the donor when accepting, shared only with the requesting hospital.
        // Cleared again when the request is declined or cancelled.
        public string? ContactNumber { get; set; }
    }
}