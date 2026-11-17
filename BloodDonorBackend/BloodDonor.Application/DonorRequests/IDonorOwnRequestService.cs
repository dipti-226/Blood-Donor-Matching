namespace BloodDonor.Application.DonorRequests
{
    public interface IDonorOwnRequestService
    {
        Task<DonorRequestResult<IReadOnlyList<DonorOwnRequestResponse>>> ListAsync(string donorUserId);

        Task<DonorRequestResult<DonorOwnRequestResponse>> AcceptAsync(string donorUserId, Guid donorRequestId, AcceptDonorRequestRequest request);

        Task<DonorRequestResult<DonorOwnRequestResponse>> DeclineAsync(string donorUserId, Guid donorRequestId);
    }
}