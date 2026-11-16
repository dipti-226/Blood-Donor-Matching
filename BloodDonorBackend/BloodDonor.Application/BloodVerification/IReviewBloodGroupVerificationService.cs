namespace BloodDonor.Application.BloodVerification
{
    public interface IReviewBloodGroupVerificationService
    {
        Task<BloodGroupReviewResult> ApproveAsync(Guid verificationId, string reviewerUserId, ApproveBloodGroupRequest request);

        Task<BloodGroupReviewResult> RejectAsync(Guid verificationId, string reviewerUserId, RejectBloodGroupRequest request);
    }
}