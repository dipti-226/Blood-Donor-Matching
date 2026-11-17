using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodRequests
{
    public interface IBloodRequestQueryService
    {
        Task<BloodRequestListResult> GetListAsync(string userId, BloodRequestStatus status);

        Task<BloodRequestResult> GetByIdAsync(string userId, Guid bloodRequestId);
    }
}