using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public class DonorBloodGroupVerificationResponse
    {
        public BloodGroup DeclaredBloodGroup { get; set; }
        public VerificationStatus Status { get; set; }
    }
}