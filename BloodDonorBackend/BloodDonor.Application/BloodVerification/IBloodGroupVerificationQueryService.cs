using BloodDonor.Domain.Entities;

namespace BloodDonor.Application.BloodVerification
{
    public interface IBloodGroupVerificationQueryService
    {
        Task<BloodGroupVerificationListResult> GetForReviewAsync(string reviewerUserId, VerificationStatus status);

        Task<DonorBloodGroupStatusResult> GetOwnAsync(string donorUserId);
    }
}