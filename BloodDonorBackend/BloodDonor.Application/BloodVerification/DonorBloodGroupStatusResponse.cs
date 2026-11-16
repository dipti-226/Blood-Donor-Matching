using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class DonorBloodGroupStatusResponse
    {
        public BloodGroup DeclaredBloodGroup { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public VerificationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}