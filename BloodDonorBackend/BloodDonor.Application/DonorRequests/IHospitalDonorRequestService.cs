using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.DonorRequests
{
    public interface IHospitalDonorRequestService
    {
        Task<DonorRequestResult<IReadOnlyList<DonorRequestResponse>>> SendAsync(string userId, Guid bloodRequestId, SendDonorRequestsRequest request);

        Task<DonorRequestResult<IReadOnlyList<DonorRequestResponse>>> ListAsync(string userId, Guid bloodRequestId, DonorRequestStatus? status);

        Task<DonorRequestResult<DonorRequestResponse>> CancelAsync(string userId, Guid bloodRequestId, Guid donorRequestId);

        Task<DonorRequestResult<DonorRequestContactResponse>> GetContactAsync(string userId, Guid bloodRequestId, Guid donorRequestId);
    }
}