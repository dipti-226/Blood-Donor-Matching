using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.Compatibility
{
    public interface IBloodCompatibilityService
    {
        IReadOnlyList<BloodGroup> GetCompatibleDonorBloodGroups(BloodGroup recipientBloodGroup, BloodComponent component);

        bool IsCompatible(BloodGroup donorBloodGroup, BloodGroup recipientBloodGroup, BloodComponent component);
    }
}