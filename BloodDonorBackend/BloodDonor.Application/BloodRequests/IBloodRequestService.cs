namespace BloodDonor.Application.BloodRequests
{
    public interface IBloodRequestService
    {
        Task<BloodRequestResult> CreateAsync(string userId, CreateBloodRequestRequest request);

        Task<BloodRequestResult> FulfillAsync(string userId, Guid bloodRequestId);

        Task<BloodRequestResult> CancelAsync(string userId, Guid bloodRequestId, CancelBloodRequestRequest request);
    }
}