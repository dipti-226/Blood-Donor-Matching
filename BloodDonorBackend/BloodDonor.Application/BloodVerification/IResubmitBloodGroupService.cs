namespace BloodDonor.Application.BloodVerification
{
    public interface IResubmitBloodGroupService
    {
        Task<ResubmitBloodGroupResult> ResubmitAsync(string donorUserId, ResubmitBloodGroupRequest request);
    }
}