using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Compatibility
{
    public class BloodCompatibilityResponse
    {
        public BloodGroup RecipientBloodGroup { get; set; }
        public BloodComponent Component { get; set; }
        public IReadOnlyList<BloodGroup> CompatibleDonorBloodGroups { get; set; } = Array.Empty<BloodGroup>();
    }
}