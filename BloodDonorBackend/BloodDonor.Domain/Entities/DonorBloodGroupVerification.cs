namespace BloodDonor.Domain.Entities
{
    public class DonorBloodGroupVerification
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public BloodGroup DeclaredBloodGroup { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public VerificationStatus Status { get; set; }
        public string? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}