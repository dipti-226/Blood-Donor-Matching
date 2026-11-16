using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class BloodGroupReviewResponse
    {
        public Guid Id { get; set; }
        public BloodGroup DeclaredBloodGroup { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public VerificationStatus Status { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}