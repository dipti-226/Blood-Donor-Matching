using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Matching
{
    public class DonorMatchResponse
    {
        // Opaque reference. It is NOT the user id. It is what the later donor request phase will use.
        public Guid DonorReference { get; set; }
        public BloodGroup VerifiedBloodGroup { get; set; }
        public string City { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public bool IsExactBloodGroupMatch { get; set; }
    }
}