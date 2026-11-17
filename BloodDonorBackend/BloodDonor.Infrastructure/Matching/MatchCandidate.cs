using BloodDonor.Domain.Entities;

namespace BloodDonor.Infrastructure.Matching
{
    internal class MatchCandidate
    {
        public Guid ProfileId { get; set; }
        public BloodGroup? VerifiedBloodGroup { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string City { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }
}