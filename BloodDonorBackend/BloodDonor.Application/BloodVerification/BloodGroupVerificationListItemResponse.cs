using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class BloodGroupVerificationListItemResponse
    {
        public Guid Id { get; set; }
        public string? DonorFullName { get; set; }
        public BloodGroup DeclaredBloodGroup { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public VerificationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}