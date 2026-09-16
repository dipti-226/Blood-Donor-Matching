namespace BloodDonor.Application.BloodVerification
{
    public interface IDonorBloodGroupVerificationService
    {
        Task<DonorBloodGroupVerificationResult> DeclareBloodGroupAsync(string userId, DeclareBloodGroupRequest request);
    }
}