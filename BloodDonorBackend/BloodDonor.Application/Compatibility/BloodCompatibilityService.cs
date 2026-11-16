using BloodDonor.Domain.Compatibility;
using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Compatibility
{
    public class BloodCompatibilityService : IBloodCompatibilityService
    {
        public IReadOnlyList<BloodGroup> GetCompatibleDonorBloodGroups(BloodGroup recipientBloodGroup, BloodComponent component)
        {
            return BloodCompatibilityRules.GetCompatibleDonorGroups(recipientBloodGroup, component);
        }

        public bool IsCompatible(BloodGroup donorBloodGroup, BloodGroup recipientBloodGroup, BloodComponent component)
        {
            return BloodCompatibilityRules
                .GetCompatibleDonorGroups(recipientBloodGroup, component)
                .Contains(donorBloodGroup);
        }
    }
}