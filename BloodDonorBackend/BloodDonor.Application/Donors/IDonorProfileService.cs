namespace BloodDonor.Application.Donors
{
    public interface IDonorProfileService
    {
        Task<DonorProfileResult> CreateProfileAsync(string userId, CreateDonorProfileRequest request);
        Task<DonorProfileResult> GetOwnProfileAsync(string userId);
    }
}